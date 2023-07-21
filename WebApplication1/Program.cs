using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<LoggingMiddleware>();

builder.Services.AddDbContext<ChatContext>(opt =>
 opt.UseSqlServer(builder.Configuration.GetConnectionString("ChatContext")));

builder.Services.AddCors(options => { options.AddDefaultPolicy(builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }); });

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ChatContext>();
    context.Database.EnsureCreated();
    // DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LoggingMiddleware>();
app.UseCors();
app.MapControllers();

app.Run();
