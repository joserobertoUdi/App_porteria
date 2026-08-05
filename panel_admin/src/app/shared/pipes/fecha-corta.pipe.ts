import { Pipe, PipeTransform } from '@angular/core';
import { formatearFecha } from '../../core/utils/fecha.util';

/** Formatea fechas ISO del backend (ISO 8601) a dd/MM/yyyy HH:mm local. */
@Pipe({ name: 'fechaCorta', standalone: true, pure: true })
export class FechaCortaPipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    return formatearFecha(valor);
  }
}