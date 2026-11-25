using Microsoft.Data.Sqlite;
using PortalWebnovels.Database;
using PortalWebnovels.Repositories.Dapper;
using PortalWebnovels.Repositories.Interfaces;
using PortalWebnovels.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var csb = new SqliteConnectionStringBuilder(
        builder.Configuration.GetConnectionString(
            "SqliteConnection"
            ) ?? "Data Source=Database/webnovel.db"
        );

csb.DataSource = Path.Combine( 
    builder.Environment.ContentRootPath, csb.DataSource
);

csb.ForeignKeys = true;
string connectionString = csb.ToString();

DatabaseInitializer.Inicializar(connectionString,
    Path.Combine(
        builder.Environment.ContentRootPath, "Database", "script.sql"
        )
    );

builder.Services.AddWebnovelServices(connectionString);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();    

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();
