using Microsoft.EntityFrameworkCore;

namespace Caesura.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<BookGenre> BookGenres => Set<BookGenre>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<ReadingProgress> ReadingProgress => Set<ReadingProgress>();
    public DbSet<UserLibrary> UserLibraries => Set<UserLibrary>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<InlineComment> InlineComments => Set<InlineComment>();
    public DbSet<BookRating> BookRatings => Set<BookRating>();
    public DbSet<Follow> Follows => Set<Follow>();

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);
    //
    //     // ── USERS ────────────────────────────────────────────
    //     modelBuilder.Entity<User>(e =>
    //     {
    //         e.HasKey(u => u.Id);
    //         e.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
    //         e.Property(u => u.Email).IsRequired().HasMaxLength(255);
    //         e.Property(u => u.Username).IsRequired().HasMaxLength(30);
    //         e.Property(u => u.DisplayName).HasMaxLength(100);
    //         e.Property(u => u.AvatarUrl).HasMaxLength(500);
    //         e.Property(u => u.CreatedAt).HasDefaultValueSql("now()");
    //         e.Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
    //
    //         e.HasIndex(u => u.Email).IsUnique();
    //         e.HasIndex(u => u.Username).IsUnique();
    //     });
    //
    //     // ── ACCOUNTS ─────────────────────────────────────────
    //     modelBuilder.Entity<Account>(e =>
    //     {
    //         e.HasKey(a => a.Id);
    //         e.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
    //         e.Property(a => a.Provider).IsRequired().HasMaxLength(20);
    //         e.Property(a => a.ProviderUserId).HasMaxLength(255);
    //         e.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
    //
    //         e.HasIndex(a => new { a.Provider, a.ProviderUserId });
    //
    //         e.HasOne(a => a.User)
    //          .WithMany(u => u.Accounts)
    //          .HasForeignKey(a => a.UserId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── GENRES ───────────────────────────────────────────
    //     modelBuilder.Entity<Genre>(e =>
    //     {
    //         e.HasKey(g => g.Id);
    //         e.Property(g => g.Name).IsRequired().HasMaxLength(50);
    //         e.HasIndex(g => g.Name).IsUnique();
    //     });
    //
    //     // ── BOOKS ─────────────────────────────────────────────
    //     modelBuilder.Entity<Book>(e =>
    //     {
    //         e.HasKey(b => b.Id);
    //         e.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");
    //         e.Property(b => b.Title).IsRequired().HasMaxLength(200);
    //         e.Property(b => b.Slug).IsRequired().HasMaxLength(220);
    //         e.Property(b => b.Language).HasMaxLength(10).HasDefaultValue("en");
    //         e.Property(b => b.Status).HasMaxLength(20).HasDefaultValue("published");
    //         e.Property(b => b.Source).HasMaxLength(20).HasDefaultValue("user");
    //         e.Property(b => b.TotalViews).HasDefaultValue(0);
    //         e.Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    //         e.Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
    //
    //         e.HasIndex(b => b.Slug).IsUnique();
    //         e.HasIndex(b => b.GutenbergId).IsUnique().HasFilter("gutenberg_id IS NOT NULL");
    //         // Partial index — only published books (mirrors what we had in Drizzle)
    //         e.HasIndex(b => b.Status).HasFilter("status = 'published'");
    //         e.HasIndex(b => b.AuthorId).HasFilter("author_id IS NOT NULL");
    //         e.HasIndex(b => b.CreatedAt);
    //
    //         e.HasOne(b => b.Author)
    //          .WithMany(u => u.AuthoredBooks)
    //          .HasForeignKey(b => b.AuthorId)
    //          .OnDelete(DeleteBehavior.SetNull);
    //     });
    //
    //     // ── BOOK AUTHORS (Gutenberg multi-author) ────────────
    //     modelBuilder.Entity<BookAuthor>(e =>
    //     {
    //         e.HasKey(ba => new { ba.BookId, ba.AuthorName });
    //         e.Property(ba => ba.AuthorName).HasMaxLength(200);
    //
    //         e.HasOne(ba => ba.Book)
    //          .WithMany(b => b.BookAuthors)
    //          .HasForeignKey(ba => ba.BookId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── BOOK GENRES ──────────────────────────────────────
    //     modelBuilder.Entity<BookGenre>(e =>
    //     {
    //         e.HasKey(bg => new { bg.BookId, bg.GenreId });
    //
    //         e.HasOne(bg => bg.Book)
    //          .WithMany(b => b.BookGenres)
    //          .HasForeignKey(bg => bg.BookId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //
    //         e.HasOne(bg => bg.Genre)
    //          .WithMany(g => g.BookGenres)
    //          .HasForeignKey(bg => bg.GenreId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── CHAPTERS ─────────────────────────────────────────
    //     modelBuilder.Entity<Chapter>(e =>
    //     {
    //         e.HasKey(c => c.Id);
    //         e.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
    //         e.Property(c => c.Title).HasMaxLength(200);
    //         e.Property(c => c.Status).HasMaxLength(20).HasDefaultValue("draft");
    //         e.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
    //         e.Property(c => c.UpdatedAt).HasDefaultValueSql("now()");
    //
    //         // Map JsonDocument to PostgreSQL jsonb
    //         e.Property(c => c.Content)
    //          .HasColumnType("jsonb")
    //          .IsRequired();
    //
    //         // Unique: one chapter number per book
    //         e.HasIndex(c => new { c.BookId, c.ChapterNumber }).IsUnique();
    //         // Partial index — only published chapters
    //         e.HasIndex(c => c.BookId).HasFilter("status = 'published'");
    //
    //         e.HasOne(c => c.Book)
    //          .WithMany(b => b.Chapters)
    //          .HasForeignKey(c => c.BookId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── READING PROGRESS ─────────────────────────────────
    //     modelBuilder.Entity<ReadingProgress>(e =>
    //     {
    //         e.HasKey(rp => rp.Id);
    //         e.Property(rp => rp.ScrollPosition).HasDefaultValue(0);
    //         e.Property(rp => rp.LastReadAt).HasDefaultValueSql("now()");
    //
    //         // One progress row per user per book
    //         e.HasIndex(rp => new { rp.UserId, rp.BookId }).IsUnique();
    //         e.HasIndex(rp => rp.UserId);
    //
    //         e.HasOne(rp => rp.User)
    //          .WithMany(u => u.ReadingProgress)
    //          .HasForeignKey(rp => rp.UserId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //
    //         e.HasOne(rp => rp.Book)
    //          .WithMany()
    //          .HasForeignKey(rp => rp.BookId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //
    //         e.HasOne(rp => rp.Chapter)
    //          .WithMany(c => c.ReadingProgress)
    //          .HasForeignKey(rp => rp.ChapterId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── USER LIBRARY ─────────────────────────────────────
    //     modelBuilder.Entity<UserLibrary>(e =>
    //     {
    //         e.HasKey(ul => new { ul.UserId, ul.BookId });
    //         e.Property(ul => ul.AddedAt).HasDefaultValueSql("now()");
    //
    //         e.HasIndex(ul => ul.UserId);
    //
    //         e.HasOne(ul => ul.User)
    //          .WithMany(u => u.Library)
    //          .HasForeignKey(ul => ul.UserId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //
    //         e.HasOne(ul => ul.Book)
    //          .WithMany(b => b.LibraryEntries)
    //          .HasForeignKey(ul => ul.BookId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── BOOKMARKS ────────────────────────────────────────
    //     modelBuilder.Entity<Bookmark>(e =>
    //     {
    //         e.HasKey(b => b.Id);
    //         e.Property(b => b.Label).HasMaxLength(100);
    //         e.Property(b => b.CreatedAt).HasDefaultValueSql("now()");
    //
    //         e.HasIndex(b => new { b.UserId, b.ChapterId }).IsUnique();
    //         e.HasIndex(b => b.UserId);
    //
    //         e.HasOne(b => b.User)
    //          .WithMany(u => u.Bookmarks)
    //          .HasForeignKey(b => b.UserId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //
    //         e.HasOne(b => b.Chapter)
    //          .WithMany(c => c.Bookmarks)
    //          .HasForeignKey(b => b.ChapterId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── INLINE COMMENTS ──────────────────────────────────
    //     modelBuilder.Entity<InlineComment>(e =>
    //     {
    //         e.HasKey(ic => ic.Id);
    //         e.Property(ic => ic.Id).HasDefaultValueSql("gen_random_uuid()");
    //         e.Property(ic => ic.QuoteText).IsRequired().HasMaxLength(500);
    //         e.Property(ic => ic.Content).IsRequired().HasMaxLength(2000);
    //         e.Property(ic => ic.CreatedAt).HasDefaultValueSql("now()");
    //
    //         e.HasIndex(ic => ic.ChapterId);
    //
    //         e.HasOne(ic => ic.Chapter)
    //          .WithMany(c => c.InlineComments)
    //          .HasForeignKey(ic => ic.ChapterId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //
    //         e.HasOne(ic => ic.User)
    //          .WithMany(u => u.InlineComments)
    //          .HasForeignKey(ic => ic.UserId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── BOOK RATINGS ─────────────────────────────────────
    //     modelBuilder.Entity<BookRating>(e =>
    //     {
    //         e.HasKey(r => new { r.UserId, r.BookId });
    //         e.Property(r => r.Rating).IsRequired();
    //         e.Property(r => r.Review).HasMaxLength(2000);
    //         e.Property(r => r.CreatedAt).HasDefaultValueSql("now()");
    //
    //         e.HasIndex(r => r.BookId);
    //
    //         e.HasOne(r => r.User)
    //          .WithMany(u => u.Ratings)
    //          .HasForeignKey(r => r.UserId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //
    //         e.HasOne(r => r.Book)
    //          .WithMany(b => b.Ratings)
    //          .HasForeignKey(r => r.BookId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    //
    //     // ── FOLLOWS ──────────────────────────────────────────
    //     modelBuilder.Entity<Follow>(e =>
    //     {
    //         e.HasKey(f => new { f.FollowerId, f.FollowingId });
    //         e.Property(f => f.CreatedAt).HasDefaultValueSql("now()");
    //
    //         e.HasIndex(f => f.FollowingId);
    //
    //         e.HasOne(f => f.Follower)
    //          .WithMany(u => u.Following)
    //          .HasForeignKey(f => f.FollowerId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //
    //         e.HasOne(f => f.Following)
    //          .WithMany(u => u.Followers)
    //          .HasForeignKey(f => f.FollowingId)
    //          .OnDelete(DeleteBehavior.Cascade);
    //     });
    // }
}