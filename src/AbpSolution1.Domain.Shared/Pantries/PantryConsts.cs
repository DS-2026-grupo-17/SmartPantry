namespace AbpSolution1.Pantries;

public static class PantryConsts
{
    /*
     * Umbral de aviso de vencimiento (RF-13/RF-15), fijado por el equipo.
     * Un ítem no consumido genera advertencia si vence dentro de los próximos 3 días,
     * incluido el día límite (hoy + 3). Los ítems ya vencidos también generan advertencia.
     * "Hoy" se calcula como la fecha UTC.
     */
    public const int ExpirationWarningThresholdDays = 3;

    // Frecuencia del worker de vencimientos: una vez por hora.
    public const int ExpirationWarningWorkerPeriodMilliseconds = 60 * 60 * 1000;
}
