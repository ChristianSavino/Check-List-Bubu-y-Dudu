using Open.Nat;

namespace CheckList.Core.Infrastructure
{
    /// <summary>
    /// Abre el puerto HTTPS en el router via UPnP al iniciar la app.
    /// Lo cierra al parar (Ctrl+C, kill, crash manejable, etc.)
    /// </summary>
    public class PortForwardingService : IHostedService
    {
        private readonly ILogger<PortForwardingService> _logger;
        private readonly int _port;
        private readonly string _name;
        private NatDevice? _device;

        public PortForwardingService(ILogger<PortForwardingService> logger, IConfiguration config)
        {
            _logger = logger;
            // Configurable desde appsettings, default 7226
            _port = config.GetValue<int>("PortForwarding:Port", 7226);
            _name = config.GetValue<string>("PortForwarding:Name", "CheckList-Bubu-Dudu") ?? "CheckList-Bubu-Dudu";
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var nat = new NatDiscoverer();
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(5000);

                _device = await nat.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                // Limpiar mapeo anterior si quedó colgado (crash previo)
                await TryRemoveMapping();

                // Crear mapeo TCP para HTTPS
                var mapping = new Mapping(Protocol.Tcp, _port, _port, 0, _name);
                await _device.CreatePortMapAsync(mapping);

                var externalIp = await _device.GetExternalIPAsync();
                _logger.LogInformation(
                    "Puerto {Port} abierto en el router. Acceso externo: https://{Ip}:{Port}",
                    _port, externalIp, _port);
            }
            catch (NatDeviceNotFoundException)
            {
                _logger.LogWarning("No se encontró un router con UPnP. El port forwarding no está activo.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo abrir el puerto {Port}. La app funciona igual en red local.", _port);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await TryRemoveMapping();
        }

        private async Task TryRemoveMapping()
        {
            if (_device == null) return;

            try
            {
                var mapping = new Mapping(Protocol.Tcp, _port, _port);
                await _device.DeletePortMapAsync(mapping);
                _logger.LogInformation("Puerto {Port} cerrado en el router.", _port);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo cerrar el puerto {Port} en el router.", _port);
            }
        }
    }
}
