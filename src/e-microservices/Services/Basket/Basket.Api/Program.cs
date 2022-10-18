using Basket.Api.GrpcServices;
using Basket.Api.Repository;
using Discount.Grpc.Protos;
using MassTransit;
using static Discount.Grpc.Protos.DiscountProtoService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigurationManager configuration = builder.Configuration;

//Configure Redis
builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = configuration.GetValue<string>("CacheSettings:ConnectionString");
});

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

//Configure Grpc
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(
    o => o.Address = new Uri(config["GrpcSettings:DiscountUrl"]));

builder.Services.AddScoped<DiscountGrpcService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

//Configure RabbitMQ
builder.Services.AddMassTransit((config) =>
{
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(configuration["EventBusSettings:HostAddress"]);
    });
});
//builder.Services.AddMassTransitHostedService();

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
