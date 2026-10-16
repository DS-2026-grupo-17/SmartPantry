using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using AbpSolution1.Authors;
using AbpSolution1.Books;
using AbpSolution1.Pantries;
using AbpSolution1.Products;
using AbpSolution1.Warnings;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace AbpSolution1.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class AbpSolution1DbContext :
    AbpDbContext<AbpSolution1DbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    public DbSet<Author> Authors { get; set; }

    public DbSet<AbpSolution1.Users.User> AppUsers { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Pantry> Pantries { get; set; }
    public DbSet<PantryWarning> PantryWarnings { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public AbpSolution1DbContext(DbContextOptions<AbpSolution1DbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        builder.Entity<Author>(b =>
        {
            b.ToTable(AbpSolution1Consts.DbTablePrefix + "Authors",
                AbpSolution1Consts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(AuthorConsts.MaxNameLength);
            b.Property(x => x.ShortBio).HasMaxLength(AuthorConsts.MaxShortBioLength);
        });

        builder.Entity<Book>(b =>
        {
            b.ToTable(AbpSolution1Consts.DbTablePrefix + "Books",
                AbpSolution1Consts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.HasOne<Author>().WithMany().HasForeignKey(x => x.AuthorId).IsRequired();
        });

        builder.Entity<AbpSolution1.Users.User>(b =>
        {
            b.ToTable(AbpSolution1Consts.DbTablePrefix + "Users",
                AbpSolution1Consts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(AbpSolution1.Users.UserConsts.MaxDisplayNameLength);
            b.Property(x => x.Email).IsRequired().HasMaxLength(AbpSolution1.Users.UserConsts.MaxEmailLength);
            b.Property(x => x.Role).IsRequired().HasMaxLength(AbpSolution1.Users.UserConsts.MaxRoleLength);
        });

        builder.Entity<Product>(b =>
        {
            b.ToTable(AbpSolution1Consts.DbTablePrefix + "Products", AbpSolution1Consts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Barcode).IsRequired().HasMaxLength(ProductConsts.MaxBarcodeLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ProductConsts.MaxNameLength);
            b.HasIndex(x => x.Barcode).IsUnique();
        });

        builder.Entity<Pantry>(b =>
        {
            b.ToTable(AbpSolution1Consts.DbTablePrefix + "Pantries", AbpSolution1Consts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<AbpSolution1.Users.User>().WithMany().HasForeignKey(x => x.OwnerId).IsRequired();
            b.HasIndex(x => x.OwnerId).IsUnique();
            b.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.PantryId).IsRequired();
        });

        builder.Entity<PantryItem>(b =>
        {
            b.ToTable(AbpSolution1Consts.DbTablePrefix + "PantryItems", AbpSolution1Consts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PantryWarning>(b =>
        {
            b.ToTable(AbpSolution1Consts.DbTablePrefix + "PantryWarnings", AbpSolution1Consts.DbSchema);
            b.ConfigureByConvention();
            b.HasOne<PantryItem>().WithMany().HasForeignKey(x => x.PantryItemId).IsRequired();
            // Clave lógica estable: una sola advertencia por ítem y tipo, aun con ejecuciones concurrentes.
            b.HasIndex(x => new { x.PantryItemId, x.Type }).IsUnique();
            b.HasIndex(x => x.OwnerId);
        });

        /* Configure your own tables/entities inside here */

        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(AbpSolution1Consts.DbTablePrefix + "YourEntities", AbpSolution1Consts.DbSchema);
        //    b.ConfigureByConvention(); //auto configure for the base class props
        //    //...
        //});
    }
}
