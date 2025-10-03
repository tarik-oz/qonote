using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Qonote.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearchForNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add SearchVector column to Notes table
            migrationBuilder.Sql(@"
                ALTER TABLE ""Notes"" 
                ADD COLUMN ""SearchVector"" tsvector;
            ");

            // 2. Create function to aggregate Section titles and Block content for a Note
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION get_note_search_content(note_id integer)
                RETURNS text AS $$
                DECLARE
                    content text;
                BEGIN
                    SELECT string_agg(
                        COALESCE(s.""Title"", '') || ' ' || COALESCE(b.""Content"", ''), 
                        ' '
                    ) INTO content
                    FROM ""Sections"" s
                    LEFT JOIN ""Blocks"" b ON b.""SectionId"" = s.""Id""
                    WHERE s.""NoteId"" = note_id 
                      AND s.""IsDeleted"" = false 
                      AND (b.""IsDeleted"" = false OR b.""IsDeleted"" IS NULL);
                    
                    RETURN COALESCE(content, '');
                END;
                $$ LANGUAGE plpgsql STABLE;
            ");

            // 3. Create function to update Note's SearchVector
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_note_search_vector()
                RETURNS trigger AS $$
                BEGIN
                    -- Weight A: CustomTitle (highest priority)
                    -- Weight B: VideoTitle, ChannelName
                    -- Weight C: Section titles + Block content
                    NEW.""SearchVector"" := 
                        setweight(to_tsvector('english', COALESCE(NEW.""CustomTitle"", '')), 'A') ||
                        setweight(to_tsvector('english', COALESCE(NEW.""VideoTitle"", '')), 'B') ||
                        setweight(to_tsvector('english', COALESCE(NEW.""ChannelName"", '')), 'B') ||
                        setweight(to_tsvector('english', get_note_search_content(NEW.""Id"")), 'C');
                    
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // 4. Create trigger: Update SearchVector when Note changes
            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_update_note_search_vector
                BEFORE INSERT OR UPDATE ON ""Notes""
                FOR EACH ROW
                EXECUTE FUNCTION update_note_search_vector();
            ");

            // 5. Create function to update parent Note when Section/Block changes
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_note_search_from_child()
                RETURNS trigger AS $$
                DECLARE
                    parent_note_id integer;
                BEGIN
                    -- Get parent Note ID
                    IF TG_TABLE_NAME = 'Blocks' THEN
                        SELECT s.""NoteId"" INTO parent_note_id
                        FROM ""Sections"" s
                        WHERE s.""Id"" = COALESCE(NEW.""SectionId"", OLD.""SectionId"");
                    ELSIF TG_TABLE_NAME = 'Sections' THEN
                        parent_note_id := COALESCE(NEW.""NoteId"", OLD.""NoteId"");
                    END IF;

                    -- Update parent Note's SearchVector
                    IF parent_note_id IS NOT NULL THEN
                        UPDATE ""Notes""
                        SET ""SearchVector"" = 
                            setweight(to_tsvector('english', COALESCE(""CustomTitle"", '')), 'A') ||
                            setweight(to_tsvector('english', COALESCE(""VideoTitle"", '')), 'B') ||
                            setweight(to_tsvector('english', COALESCE(""ChannelName"", '')), 'B') ||
                            setweight(to_tsvector('english', get_note_search_content(""Id"")), 'C')
                        WHERE ""Id"" = parent_note_id;
                    END IF;

                    RETURN COALESCE(NEW, OLD);
                END;
                $$ LANGUAGE plpgsql;
            ");

            // 6. Create triggers for Section and Block changes
            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_update_note_from_section
                AFTER INSERT OR UPDATE OR DELETE ON ""Sections""
                FOR EACH ROW
                EXECUTE FUNCTION update_note_search_from_child();
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_update_note_from_block
                AFTER INSERT OR UPDATE OR DELETE ON ""Blocks""
                FOR EACH ROW
                EXECUTE FUNCTION update_note_search_from_child();
            ");

            // 7. Create GIN index for fast full-text search
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_Notes_SearchVector"" 
                ON ""Notes"" 
                USING GIN(""SearchVector"");
            ");

            // 8. Populate SearchVector for existing Notes
            migrationBuilder.Sql(@"
                UPDATE ""Notes""
                SET ""SearchVector"" = 
                    setweight(to_tsvector('english', COALESCE(""CustomTitle"", '')), 'A') ||
                    setweight(to_tsvector('english', COALESCE(""VideoTitle"", '')), 'B') ||
                    setweight(to_tsvector('english', COALESCE(""ChannelName"", '')), 'B') ||
                    setweight(to_tsvector('english', get_note_search_content(""Id"")), 'C')
                WHERE ""SearchVector"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Drop GIN index
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Notes_SearchVector"";");

            // 2. Drop triggers
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trigger_update_note_from_block ON ""Blocks"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trigger_update_note_from_section ON ""Sections"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trigger_update_note_search_vector ON ""Notes"";");

            // 3. Drop functions
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS update_note_search_from_child();");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS update_note_search_vector();");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS get_note_search_content(integer);");

            // 4. Drop SearchVector column
            migrationBuilder.Sql(@"ALTER TABLE ""Notes"" DROP COLUMN IF EXISTS ""SearchVector"";");
        }
    }
}
