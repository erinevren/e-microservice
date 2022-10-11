using Basket.Api.GrpcServices;
using Basket.Api.Repository;
using Discount.Grpc.Protos;
using static Discount.Grpc.Protos.DiscountProtoService;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigurationManager configuration = builder.Configuration;

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = configuration.GetValue<string>("CacheSettings:ConnectionString");
});

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(
    o => o.Address = new Uri(config["GrpcSettings:DiscountUrl"]));

builder.Services.AddScoped<DiscountGrpcService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
