using Uno.Files.Options.Viewer;
using Uno.Files.Viewer.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor(); // Required UnoViewer
builder.Services.AddMemoryCache(); // Required UnoViewer


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// UnoViewer Start
app.MapWhen(context => context.Request.Path.ToString().EndsWith("UnoImage.axd"),
    appBranch =>
    {
        appBranch.UseUnoViewer(new UnoViewOptions { UnSafe = false, ShowInfo = false });
    }
);
// UnoViewer End

app.Run();
