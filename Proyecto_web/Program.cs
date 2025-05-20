using Proyecto_web.Services;

var builder = WebApplication.CreateBuilder(args);

// ?? Esto ya viene por defecto: agrega la configuraci�n
builder.Services.AddRazorPages();
builder.Services.AddSingleton<MenuService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
// ?? Si quieres usar Session m�s adelante:
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization(); // si usas autenticaci�n
app.UseSession();       // si vas a usar sesiones

app.MapRazorPages();

app.Run();
