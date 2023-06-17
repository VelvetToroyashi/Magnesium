using Magnesium.Data;
using Magnesium.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;

var config = new ConfigurationBuilder()
.AddEnvironmentVariables();

var services = new ServiceCollection();

services
.AddSingleton(config.Build())
.AddDiscordGateway(s => s.GetRequiredService<IConfiguration>()["token"] ?? throw new KeyNotFoundException("Token was not set!"))
.AddDbContext<MagnesiumContext>()
.AddResponder<ReadStateService>()
.AddDiscordCommands(true);

var provider = services.BuildServiceProvider();

await provider.GetRequiredService<DiscordGatewayClient>().RunAsync(default);