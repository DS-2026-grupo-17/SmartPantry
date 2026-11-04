import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/*
 * La fecha de vencimiento es una fecha de calendario (DateOnly en el BackEnd, "yyyy-MM-dd").
 * No se convierte entre zonas horarias: se muestra el mismo día que guardó el servidor.
 * El worker del TP08 compara esa fecha con el día UTC actual.
 */
export const MIN_EXPIRATION_YEAR = 2000;
export const MAX_EXPIRATION_YEAR = 2100;

const DATE_ONLY_PATTERN = /^(\d{4})-(\d{2})-(\d{2})$/;

// "2026-10-23" -> Date local del 23/10/2026 (evita que new Date("2026-10-23") lo corra un día por UTC).
export function toCalendarDate(value: string | null | undefined): Date | null {
  const match = value ? DATE_ONLY_PATTERN.exec(value) : null;
  if (!match) {
    return null;
  }

  const [year, month, day] = [Number(match[1]), Number(match[2]), Number(match[3])];
  const date = new Date(year, month - 1, day);
  const isRealDate = date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;
  return isRealDate ? date : null;
}

// Vacío es válido (la fecha es opcional). Si hay valor, debe ser una fecha real entre 2000 y 2100.
export const validExpirationDate: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value = control.value as string | null;
  if (!value) {
    return null;
  }

  const date = toCalendarDate(value);
  const year = date?.getFullYear() ?? 0;
  return date && year >= MIN_EXPIRATION_YEAR && year <= MAX_EXPIRATION_YEAR ? null : { invalidDate: true };
};
