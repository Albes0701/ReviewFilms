using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<MovieCredit> MovieCredits => Set<MovieCredit>();

    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();

    public DbSet<MovieRating> MovieRatings => Set<MovieRating>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<CommentVote> CommentVotes => Set<CommentVote>();

    public DbSet<Watchlist> Watchlists => Set<Watchlist>();

    public DbSet<Report> Reports => Set<Report>();

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurePostgresEnums(modelBuilder);

        ConfigureUser(modelBuilder.Entity<User>());
        ConfigureRole(modelBuilder.Entity<Role>());
        ConfigurePermission(modelBuilder.Entity<Permission>());
        ConfigureRolePermission(modelBuilder.Entity<RolePermission>());
        ConfigureUserRole(modelBuilder.Entity<UserRole>());
        ConfigureRefreshToken(modelBuilder.Entity<RefreshToken>());
        ConfigureMovie(modelBuilder.Entity<Movie>());
        ConfigureGenre(modelBuilder.Entity<Genre>());
        ConfigurePerson(modelBuilder.Entity<Person>());
        ConfigureMovieCredit(modelBuilder.Entity<MovieCredit>());
        ConfigureMovieGenre(modelBuilder.Entity<MovieGenre>());
        ConfigureMovieRating(modelBuilder.Entity<MovieRating>());
        ConfigureComment(modelBuilder.Entity<Comment>());
        ConfigureCommentVote(modelBuilder.Entity<CommentVote>());
        ConfigureWatchlist(modelBuilder.Entity<Watchlist>());
        ConfigureReport(modelBuilder.Entity<Report>());
        ConfigureNotification(modelBuilder.Entity<Notification>());

        ApplyTemporalColumnTypes(modelBuilder);
    }

    private static void ConfigurePostgresEnums(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<UserStatus>();
        modelBuilder.HasPostgresEnum<MovieStatus>();
        modelBuilder.HasPostgresEnum<CreditType>();
        modelBuilder.HasPostgresEnum<CommentStatus>();
        modelBuilder.HasPostgresEnum<WatchlistStatus>();
        modelBuilder.HasPostgresEnum<ReportTargetType>();
        modelBuilder.HasPostgresEnum<ReportStatus>();
        modelBuilder.HasPostgresEnum<NotificationType>();
        modelBuilder.HasPostgresEnum<VoteType>();
    }

    private static void ConfigureUser(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("user");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
        entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.AvatarUrl).HasMaxLength(500);
        entity.Property(x => x.Bio);

        entity.HasIndex(x => x.Username).IsUnique();
        entity.HasIndex(x => x.Email).IsUnique();

        entity.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.AssignedUserRoles)
            .WithOne(x => x.AssignedByUser)
            .HasForeignKey(x => x.AssignedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.CreatedRolePermissions)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.CreatedMovies)
            .WithOne(x => x.CreatedByUser)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.MovieRatings)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.Comments)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.CommentVotes)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.Watchlists)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.ReportsFiled)
            .WithOne(x => x.ReporterUser)
            .HasForeignKey(x => x.ReporterUserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.ReportsReviewed)
            .WithOne(x => x.ReviewedByUser)
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.Notifications)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureRole(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("role");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Description);

        entity.HasIndex(x => x.Code).IsUnique();

        entity.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigurePermission(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("permission");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Module).HasMaxLength(100);
        entity.Property(x => x.Description);

        entity.HasIndex(x => x.Code).IsUnique();

        entity.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureRolePermission(EntityTypeBuilder<RolePermission> entity)
    {
        entity.ToTable("role_permission");

        entity.HasKey(x => new { x.RoleId, x.PermissionId });

        entity.Property(x => x.CreatedByUserId).HasColumnName("created_by");

        entity.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.Permission)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedRolePermissions)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureUserRole(EntityTypeBuilder<UserRole> entity)
    {
        entity.ToTable("user_role");

        entity.HasKey(x => new { x.UserId, x.RoleId });

        entity.Property(x => x.AssignedByUserId).HasColumnName("assigned_by");

        entity.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.AssignedByUser)
            .WithMany(x => x.AssignedUserRoles)
            .HasForeignKey(x => x.AssignedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureRefreshToken(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.ToTable("refresh_token");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.TokenHash).HasMaxLength(500).IsRequired();
        entity.Property(x => x.JwtId).HasMaxLength(255);
        entity.Property(x => x.DeviceName).HasMaxLength(255);
        entity.Property(x => x.IpAddress).HasMaxLength(64);
        entity.Property(x => x.UserAgent);

        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => x.TokenHash).IsUnique();
        entity.HasIndex(x => x.JwtId);

        entity.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.ReplacedByToken)
            .WithMany(x => x.ReplacedTokens)
            .HasForeignKey(x => x.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureMovie(EntityTypeBuilder<Movie> entity)
    {
        entity.ToTable("movie");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Title).HasMaxLength(255).IsRequired();
        entity.Property(x => x.OriginalTitle).HasMaxLength(255);
        entity.Property(x => x.Slug).HasMaxLength(255).IsRequired();
        entity.Property(x => x.Overview);
        entity.Property(x => x.AgeRating).HasMaxLength(20);
        entity.Property(x => x.OriginalLanguage).HasMaxLength(10);
        entity.Property(x => x.PosterUrl).HasMaxLength(500);
        entity.Property(x => x.BackdropUrl).HasMaxLength(500);
        entity.Property(x => x.TrailerUrl).HasMaxLength(500);
        entity.Property(x => x.AvgRating).HasPrecision(3, 2);
        entity.Property(x => x.CreatedByUserId).HasColumnName("created_by");

        entity.HasIndex(x => x.Slug).IsUnique();
        entity.HasIndex(x => x.Title);
        entity.HasIndex(x => x.ReleaseDate);
        entity.HasIndex(x => x.Status);

        entity.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.CreatedMovies)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.MovieCredits)
            .WithOne(x => x.Movie)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.MovieGenres)
            .WithOne(x => x.Movie)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.MovieRatings)
            .WithOne(x => x.Movie)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.Comments)
            .WithOne(x => x.Movie)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.Watchlists)
            .WithOne(x => x.Movie)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureGenre(EntityTypeBuilder<Genre> entity)
    {
        entity.ToTable("genres");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Description);

        entity.HasIndex(x => x.Name).IsUnique();
        entity.HasIndex(x => x.Slug).IsUnique();

        entity.HasMany(x => x.MovieGenres)
            .WithOne(x => x.Genre)
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigurePerson(EntityTypeBuilder<Person> entity)
    {
        entity.ToTable("persons");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Name).HasMaxLength(255).IsRequired();
        entity.Property(x => x.OriginalName).HasMaxLength(255);
        entity.Property(x => x.KnownForDepartment).HasMaxLength(100);
        entity.Property(x => x.Gender).HasMaxLength(20);
        entity.Property(x => x.PlaceOfBirth).HasMaxLength(255);
        entity.Property(x => x.ProfileUrl).HasMaxLength(500);
        entity.Property(x => x.Biography);

        entity.HasIndex(x => x.Name);

        entity.HasMany(x => x.MovieCredits)
            .WithOne(x => x.Person)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureMovieCredit(EntityTypeBuilder<MovieCredit> entity)
    {
        entity.ToTable("moviecredits");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Department).HasMaxLength(100);
        entity.Property(x => x.Job).HasMaxLength(100);
        entity.Property(x => x.CharacterName).HasMaxLength(255);

        entity.HasIndex(x => x.MovieId);
        entity.HasIndex(x => x.PersonId);
        entity.HasIndex(x => new { x.MovieId, x.PersonId, x.CreditType, x.Job, x.CharacterName });

        entity.HasOne(x => x.Movie)
            .WithMany(x => x.MovieCredits)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.Person)
            .WithMany(x => x.MovieCredits)
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureMovieGenre(EntityTypeBuilder<MovieGenre> entity)
    {
        entity.ToTable("movie_genres");

        entity.HasKey(x => new { x.MovieId, x.GenreId });

        entity.HasOne(x => x.Movie)
            .WithMany(x => x.MovieGenres)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.Genre)
            .WithMany(x => x.MovieGenres)
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureMovieRating(EntityTypeBuilder<MovieRating> entity)
    {
        entity.ToTable("movie_rating");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Score).HasComment("Suggested range: 1..10");

        entity.HasIndex(x => x.MovieId);
        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => new { x.UserId, x.MovieId }).IsUnique();

        entity.HasOne(x => x.Movie)
            .WithMany(x => x.MovieRatings)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.User)
            .WithMany(x => x.MovieRatings)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureComment(EntityTypeBuilder<Comment> entity)
    {
        entity.ToTable("comment");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Content).IsRequired();

        entity.HasIndex(x => x.MovieId);
        entity.HasIndex(x => x.ParentId);
        entity.HasIndex(x => x.RootId);
        entity.HasIndex(x => new { x.MovieId, x.RootId, x.CreatedAt });

        entity.HasOne(x => x.Movie)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.User)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.ParentComment)
            .WithMany(x => x.ChildComments)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.RootComment)
            .WithMany(x => x.ThreadComments)
            .HasForeignKey(x => x.RootId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasMany(x => x.CommentVotes)
            .WithOne(x => x.Comment)
            .HasForeignKey(x => x.CommentId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureCommentVote(EntityTypeBuilder<CommentVote> entity)
    {
        entity.ToTable("comment_vote");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.HasIndex(x => x.CommentId);
        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => new { x.CommentId, x.UserId }).IsUnique();

        entity.HasOne(x => x.Comment)
            .WithMany(x => x.CommentVotes)
            .HasForeignKey(x => x.CommentId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.User)
            .WithMany(x => x.CommentVotes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureWatchlist(EntityTypeBuilder<Watchlist> entity)
    {
        entity.ToTable("watchlist");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Note);

        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => x.MovieId);
        entity.HasIndex(x => new { x.UserId, x.MovieId }).IsUnique();
        entity.HasIndex(x => new { x.UserId, x.Status });

        entity.HasOne(x => x.User)
            .WithMany(x => x.Watchlists)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.Movie)
            .WithMany(x => x.Watchlists)
            .HasForeignKey(x => x.MovieId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureReport(EntityTypeBuilder<Report> entity)
    {
        entity.ToTable("report");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.ReasonCode).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Description);
        entity.Property(x => x.ResolutionNote);
        entity.Property(x => x.ReviewedByUserId).HasColumnName("reviewed_by");

        entity.HasIndex(x => x.ReporterUserId);
        entity.HasIndex(x => x.ReviewedByUserId);
        entity.HasIndex(x => new { x.TargetType, x.TargetId });
        entity.HasIndex(x => new { x.TargetType, x.TargetId, x.Status });

        entity.HasOne(x => x.ReporterUser)
            .WithMany(x => x.ReportsFiled)
            .HasForeignKey(x => x.ReporterUserId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne(x => x.ReviewedByUser)
            .WithMany(x => x.ReportsReviewed)
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureNotification(EntityTypeBuilder<Notification> entity)
    {
        entity.ToTable("notification");

        entity.HasKey(x => x.Id);
        entity.Property(x => x.Id).ValueGeneratedNever();

        entity.Property(x => x.Title).HasMaxLength(255).IsRequired();
        entity.Property(x => x.Message).IsRequired();
        entity.Property(x => x.DataJson).HasColumnType("jsonb");

        entity.HasIndex(x => x.UserId);
        entity.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });

        entity.HasOne(x => x.User)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ApplyTemporalColumnTypes(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

                if (clrType == typeof(DateTime))
                {
                    property.SetColumnType("timestamp without time zone");
                }
                else if (clrType == typeof(DateOnly))
                {
                    property.SetColumnType("date");
                }
            }
        }
    }
}
