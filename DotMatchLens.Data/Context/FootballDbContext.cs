using DotMatchLens.Data.Entities;

namespace DotMatchLens.Data.Context;

/// <summary>
/// Main database context for DotMatchLens application.
/// Configured with NoTracking by default for read-heavy operations.
/// </summary>
public sealed class FootballDbContext : DbContext
{
    public FootballDbContext(DbContextOptions<FootballDbContext> options)
        : base(options)
    {
        // Default to NoTracking for read-heavy operations
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<MatchPrediction> MatchPredictions => Set<MatchPrediction>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<Season> Seasons => Set<Season>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");

        // Configure entities
        ConfigureTeam(modelBuilder);
        ConfigurePlayer(modelBuilder);
        ConfigureMatch(modelBuilder);
        ConfigureMatchEvent(modelBuilder);
        ConfigureMatchPrediction(modelBuilder);
        ConfigureCompetition(modelBuilder);
        ConfigureSeason(modelBuilder);
    }

    private static void ConfigureTeam(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ShortName).HasMaxLength(100);
            entity.Property(e => e.Tla).HasMaxLength(10);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.League).HasMaxLength(200);
            entity.Property(e => e.Crest).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.ClubColors).HasMaxLength(100);
            entity.Property(e => e.Venue).HasMaxLength(200);
            entity.Property(e => e.RawJson).HasColumnType("jsonb");
            entity.Property(e => e.Embedding).HasColumnType("vector(768)");
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.ExternalId);
        });
    }

    private static void ConfigurePlayer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("players");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Players)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureMatch(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("matches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Stadium).HasMaxLength(200);
            entity.HasOne(e => e.HomeTeam)
                  .WithMany(t => t.HomeMatches)
                  .HasForeignKey(e => e.HomeTeamId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AwayTeam)
                  .WithMany(t => t.AwayMatches)
                  .HasForeignKey(e => e.AwayTeamId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.MatchDate);
        });
    }

    private static void ConfigureMatchEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatchEvent>(entity =>
        {
            entity.ToTable("match_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.Match)
                  .WithMany(m => m.Events)
                  .HasForeignKey(e => e.MatchId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Player)
                  .WithMany()
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureMatchPrediction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MatchPrediction>(entity =>
        {
            entity.ToTable("match_predictions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ModelVersion).HasMaxLength(50);
            entity.Property(e => e.Reasoning).HasMaxLength(2000);
            // Configure vector column for embeddings
            entity.Property(e => e.ContextEmbedding)
                  .HasColumnType("vector(1536)");
            entity.HasOne(e => e.Match)
                  .WithMany(m => m.Predictions)
                  .HasForeignKey(e => e.MatchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCompetition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Competition>(entity =>
        {
            entity.ToTable("competitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Emblem).HasMaxLength(500);
            entity.Property(e => e.AreaName).HasMaxLength(100);
            entity.Property(e => e.AreaCode).HasMaxLength(10);
            entity.Property(e => e.AreaFlag).HasMaxLength(500);
            entity.Property(e => e.RawJson).HasColumnType("jsonb");
            entity.Property(e => e.Embedding).HasColumnType("vector(768)");
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.ExternalId).IsUnique();
        });
    }

    private static void ConfigureSeason(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Season>(entity =>
        {
            entity.ToTable("seasons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WinnerName).HasMaxLength(200);
            entity.Property(e => e.RawJson).HasColumnType("jsonb");
            entity.Property(e => e.Embedding).HasColumnType("vector(768)");
            entity.HasOne(e => e.Competition)
                  .WithMany(c => c.Seasons)
                  .HasForeignKey(e => e.CompetitionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ExternalId);
            entity.HasIndex(e => new { e.CompetitionId, e.StartDate });
        });
    }
}
