using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReviewFilms.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Create_Pomelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "genres",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_genres", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    module = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "longtext", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    original_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    known_for_department = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    gender = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    birthday = table.Column<DateTime>(type: "date", nullable: true),
                    deathday = table.Column<DateTime>(type: "date", nullable: true),
                    place_of_birth = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    biography = table.Column<string>(type: "longtext", nullable: true),
                    profile_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true),
                    is_system = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    username = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    display_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    avatar_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    bio = table.Column<string>(type: "longtext", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    email_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "movie",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    original_title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    slug = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    overview = table.Column<string>(type: "longtext", nullable: true),
                    release_date = table.Column<DateTime>(type: "date", nullable: true),
                    runtime_minutes = table.Column<int>(type: "int", nullable: true),
                    age_rating = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    original_language = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    poster_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    backdrop_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    trailer_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    avg_rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    rating_count = table.Column<int>(type: "int", nullable: false),
                    comment_count = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movie", x => x.id);
                    table.ForeignKey(
                        name: "fk_movie_user_created_by_user_id",
                        column: x => x.created_by,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "longtext", nullable: false),
                    data_json = table.Column<string>(type: "json", nullable: true),
                    is_read = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    token_hash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    jwt_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    device_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    ip_address = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "longtext", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    replaced_by_token_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_token_refresh_token_replaced_by_token_id",
                        column: x => x.replaced_by_token_id,
                        principalTable: "refresh_token",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refresh_token_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "report",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    reporter_user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    target_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    target_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    reason_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    reviewed_by = table.Column<Guid>(type: "char(36)", nullable: true),
                    resolution_note = table.Column<string>(type: "longtext", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report", x => x.id);
                    table.ForeignKey(
                        name: "fk_report_user_reporter_user_id",
                        column: x => x.reporter_user_id,
                        principalTable: "user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_report_user_reviewed_by_user_id",
                        column: x => x.reviewed_by,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    permission_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    created_by = table.Column<Guid>(type: "char(36)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permission", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permission_permission_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_role_permission_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_role_permission_user_created_by_user_id",
                        column: x => x.created_by,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    role_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    assigned_by = table.Column<Guid>(type: "char(36)", nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_role_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_role_user_assigned_by_user_id",
                        column: x => x.assigned_by,
                        principalTable: "user",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_user_role_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "comment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    movie_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    parent_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    root_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    content = table.Column<string>(type: "longtext", nullable: false),
                    depth = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    upvote_count = table.Column<int>(type: "int", nullable: false),
                    downvote_count = table.Column<int>(type: "int", nullable: false),
                    reply_count = table.Column<int>(type: "int", nullable: false),
                    is_edited = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    edited_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comment", x => x.id);
                    table.ForeignKey(
                        name: "fk_comment_comment_parent_id",
                        column: x => x.parent_id,
                        principalTable: "comment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_comment_comment_root_id",
                        column: x => x.root_id,
                        principalTable: "comment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_comment_movie_movie_id",
                        column: x => x.movie_id,
                        principalTable: "movie",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_comment_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "movie_genres",
                columns: table => new
                {
                    movie_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    genre_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movie_genres", x => new { x.movie_id, x.genre_id });
                    table.ForeignKey(
                        name: "fk_movie_genres_genres_genre_id",
                        column: x => x.genre_id,
                        principalTable: "genres",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_movie_genres_movies_movie_id",
                        column: x => x.movie_id,
                        principalTable: "movie",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "movie_rating",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    movie_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false, comment: "Suggested range: 1..10"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movie_rating", x => x.id);
                    table.ForeignKey(
                        name: "fk_movie_rating_movie_movie_id",
                        column: x => x.movie_id,
                        principalTable: "movie",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_movie_rating_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "moviecredits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    movie_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    person_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    credit_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    department = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    job = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    character_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    billing_order = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moviecredits", x => x.id);
                    table.ForeignKey(
                        name: "fk_moviecredits_movie_movie_id",
                        column: x => x.movie_id,
                        principalTable: "movie",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_moviecredits_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "watchlist",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    movie_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    priority = table.Column<int>(type: "int", nullable: true),
                    note = table.Column<string>(type: "longtext", nullable: true),
                    added_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    watched_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_watchlist", x => x.id);
                    table.ForeignKey(
                        name: "fk_watchlist_movie_movie_id",
                        column: x => x.movie_id,
                        principalTable: "movie",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_watchlist_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "comment_vote",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    comment_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    vote_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comment_vote", x => x.id);
                    table.ForeignKey(
                        name: "fk_comment_vote_comment_comment_id",
                        column: x => x.comment_id,
                        principalTable: "comment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_comment_vote_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_comment_movie_id",
                table: "comment",
                column: "movie_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_movie_id_root_id_created_at",
                table: "comment",
                columns: new[] { "movie_id", "root_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_comment_parent_id",
                table: "comment",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_root_id",
                table: "comment",
                column: "root_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_user_id",
                table: "comment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_vote_comment_id",
                table: "comment_vote",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_comment_vote_comment_id_user_id",
                table: "comment_vote",
                columns: new[] { "comment_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_comment_vote_user_id",
                table: "comment_vote",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_genres_name",
                table: "genres",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_genres_slug",
                table: "genres",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movie_created_by_user_id",
                table: "movie",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_movie_release_date",
                table: "movie",
                column: "release_date");

            migrationBuilder.CreateIndex(
                name: "ix_movie_slug",
                table: "movie",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movie_status",
                table: "movie",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_movie_title",
                table: "movie",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_movie_genres_genre_id",
                table: "movie_genres",
                column: "genre_id");

            migrationBuilder.CreateIndex(
                name: "ix_movie_rating_movie_id",
                table: "movie_rating",
                column: "movie_id");

            migrationBuilder.CreateIndex(
                name: "ix_movie_rating_user_id",
                table: "movie_rating",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_movie_rating_user_id_movie_id",
                table: "movie_rating",
                columns: new[] { "user_id", "movie_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_moviecredits_movie_id",
                table: "moviecredits",
                column: "movie_id");

            migrationBuilder.CreateIndex(
                name: "ix_moviecredits_movie_id_person_id_credit_type_job_character_na",
                table: "moviecredits",
                columns: new[] { "movie_id", "person_id", "credit_type", "job", "character_name" });

            migrationBuilder.CreateIndex(
                name: "ix_moviecredits_person_id",
                table: "moviecredits",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_user_id",
                table: "notification",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_user_id_is_read_created_at",
                table: "notification",
                columns: new[] { "user_id", "is_read", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_permission_code",
                table: "permission",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_persons_name",
                table: "persons",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_jwt_id",
                table: "refresh_token",
                column: "jwt_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_replaced_by_token_id",
                table: "refresh_token",
                column: "replaced_by_token_id");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_token_hash",
                table: "refresh_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user_id",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_reporter_user_id",
                table: "report",
                column: "reporter_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_reviewed_by_user_id",
                table: "report",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "ix_report_target_type_target_id",
                table: "report",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_report_target_type_target_id_status",
                table: "report",
                columns: new[] { "target_type", "target_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_role_code",
                table: "role",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_created_by_user_id",
                table: "role_permission",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_permission_id",
                table: "role_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_email",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_username",
                table: "user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_role_assigned_by_user_id",
                table: "user_role",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "ix_user_role_role_id",
                table: "user_role",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_movie_id",
                table: "watchlist",
                column: "movie_id");

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_user_id",
                table: "watchlist",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_user_id_movie_id",
                table: "watchlist",
                columns: new[] { "user_id", "movie_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_user_id_status",
                table: "watchlist",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comment_vote");

            migrationBuilder.DropTable(
                name: "movie_genres");

            migrationBuilder.DropTable(
                name: "movie_rating");

            migrationBuilder.DropTable(
                name: "moviecredits");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "report");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "watchlist");

            migrationBuilder.DropTable(
                name: "comment");

            migrationBuilder.DropTable(
                name: "genres");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "movie");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
