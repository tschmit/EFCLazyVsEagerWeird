using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

string cs = "Data Source=WKS003\\SQL140EXP;Initial Catalog=testEFcore;Integrated Security=True;Connect Timeout=180;MultipleActiveResultSets=False;Encrypt=False";

using BlogCtx ctx = new BlogCtx(cs);
ctx.Database.Migrate();

Blog? b = null;

if (ctx.Set<Blog>().Count() == 0 ) {
    b = new Blog { 
        Title = "Le premier"
    };    
    ctx.Add(b);
    await ctx.SaveChangesAsync();
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
    _ = await ctx.Database.ExecuteSqlRawAsync($"insert into Logs (Date, Text, Blogid) values ('20240101', 'test with null IsError', {b.Id})");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
} else {

    b = await ctx.Set<Blog>().Include(x => x.Logs).Where(x => x.Id == 1).FirstOrDefaultAsync();
    Console.WriteLine(b.Logs.Count());

    try {
        using BlogCtx ctx2 = new BlogCtx(cs);
        b = await ctx2.Set<Blog>().Where(x => x.Id == 1).FirstOrDefaultAsync();
        Console.WriteLine(b.Logs.Count());
    } catch (Exception ex) {
        Console.WriteLine(ex.ToString());
    }
}

 
public class BlogCtx : DbContext {
    public string _cs;
    private bool _forMigration { get; set; }

    public BlogCtx() {
        _forMigration = true;
        _cs = null;
    }

    public BlogCtx(string cs) {
        _cs = cs;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder b) {
        if (_forMigration || !string.IsNullOrEmpty(_cs)) {
            b.
                UseLazyLoadingProxies().
                UseSqlServer(_cs);
        }
        base.OnConfiguring(b);
    }

    protected override void OnModelCreating(ModelBuilder b) {
        new BlogConfiguration().Configure(b.Entity<Blog>());
        new PostConfiguration().Configure(b.Entity<Post>());
        new LogConfiguration().Configure(b.Entity<Log>());

        base.OnModelCreating(b);
    }
}

public class  BlogConfiguration : IEntityTypeConfiguration<Blog> {
    public void Configure(EntityTypeBuilder<Blog> b) {
        b.ToTable("Blogs", "dbo");
        b.HasKey(x => x.Id);
    }
}

public class PostConfiguration : IEntityTypeConfiguration<Post> {
    public void Configure(EntityTypeBuilder<Post> b) {
        b.ToTable("Posts", "dbo");
        b.HasKey(x => x.Id);
    }
}

public class LogConfiguration : IEntityTypeConfiguration<Log> {
    public void Configure(EntityTypeBuilder<Log> b) {
        b.ToTable("Logs", "dbo");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Blog).WithMany(y => y.Logs).HasForeignKey(x => x.BlogId);
        b.HasOne(x => x.Post).WithMany(y => y.Logs).HasForeignKey(x => x.PostId);
    }    
}

public class Blog {
    public Blog () {
        Posts = new List<Post>();
        Logs = new List<Log>();
    }
    public int Id { get; set; }
    public string Title { get; set; }

    public virtual ICollection<Post> Posts { get; set; }
    public virtual ICollection<Log> Logs { get; set; }
}

public class Post {
    public int Id { get; set; }
    public int BlogId { get; set; }
    public virtual Blog Blog { get; set; }
    public string Text { get; set; }
    public virtual ICollection<Log> Logs { get; set; }
}

public class Log {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Text { get; set; }
    public int? BlogId { get; set; }
    public virtual Blog Blog { get; set; }
    public int? PostId { get; set; }
    public virtual Post Post { get; set; }
    public bool IsError { get; set; }
}