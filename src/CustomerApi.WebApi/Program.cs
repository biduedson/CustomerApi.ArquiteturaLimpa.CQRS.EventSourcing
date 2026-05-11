

using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services
   .Configure<GzipCompressionProviderOptions>(compressionOptions => compressionOptions.Level = CompressionLevel.Fastest);


        // Add services to the container.
 builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
 builder.Services.AddOpenApi();

 var app = builder.Build();

        // Configure the HTTP request pipeline.
 if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

 app.UseHttpsRedirection();

 app.UseAuthorization();


 app.MapControllers();

 app.Run();
    

