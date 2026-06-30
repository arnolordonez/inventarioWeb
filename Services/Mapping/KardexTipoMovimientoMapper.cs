using InventarioWEB.Models.Enums;

namespace InventarioWEB.Services.Mapping
{
    public static class KardexTipoMovimientoMapper
    {
        public static TipoMovimientoKardex? GetTipo(string tipoMovimiento)
        {
            if (string.IsNullOrWhiteSpace(tipoMovimiento))
                return null;

            tipoMovimiento = tipoMovimiento.Trim().ToUpperInvariant();

            return tipoMovimiento switch
            {
                "PRODUCCION" => TipoMovimientoKardex.Entrada,
                "AJUSTE" => TipoMovimientoKardex.Entrada,
                "AJUSTE_POSITIVO" => TipoMovimientoKardex.Entrada,
                "DEVOLUCION_CLIENTE" => TipoMovimientoKardex.Entrada,
                "ENTRADA" => TipoMovimientoKardex.Entrada,

                "VENTA" => TipoMovimientoKardex.Salida,
                "VENTA_DESPACHO" => TipoMovimientoKardex.Salida,
                "DESPACHO" => TipoMovimientoKardex.Salida,
                "AJUSTE_NEGATIVO" => TipoMovimientoKardex.Salida,
                "SALIDA" => TipoMovimientoKardex.Salida,

                _ => null
            };
        }
    }
}