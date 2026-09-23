import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { formatearFecha, iniciales } from '../../../core/utils/fecha.util';
import { rolVisible as rolLegible } from '../../../core/utils/rol.util';
import { AprobacionParqueo, AprobacionPorteria, AprobacionesOperador } from '../../../domain/models/aprobaciones';
import { UsuarioLista } from '../../../domain/models/usuario';
import { UdiEmptyComponent } from '../../../shared/components/udi-empty/udi-empty.component';
import { UdiLoadingComponent } from '../../../shared/components/udi-loading/udi-loading.component';
import { AdminService } from '../../services/admin.service';

@Component({
  selector: 'app-auditoria-guardia',
  standalone: true,
  imports: [UdiLoadingComponent, UdiEmptyComponent],
  templateUrl: './auditoria-guardia.component.html',
  styleUrl: './auditoria-guardia.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditoriaGuardiaComponent implements OnInit {
  readonly guardia = input.required<UsuarioLista>();
  readonly volver = output<void>();

  private readonly admin = inject(AdminService);
  private readonly destroyRef = inject(DestroyRef);

  /** Día seleccionado en formato yyyy-MM-dd (null = sin filtro). */
  readonly dia = signal<string | null>(null);
  readonly cargando = signal(true);
  readonly datos = signal<AprobacionesOperador | null>(null);
  readonly error = signal<string | null>(null);

  private orden = 0;

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    const orden = ++this.orden;
    this.cargando.set(true);
    this.error.set(null);
    this.admin
      .obtenerAprobacionesOperador(this.guardia().id, this.dia() ?? undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (d) => {
          if (orden !== this.orden) {
            return;
          }
          this.datos.set(d);
          this.cargando.set(false);
        },
        error: () => {
          if (orden !== this.orden) {
            return;
          }
          this.cargando.set(false);
          this.error.set('No se pudieron cargar las aprobaciones del guardia.');
        },
      });
  }

  cambiarDia(evento: Event): void {
    const valor = (evento.target as HTMLInputElement).value;
    this.dia.set(valor === '' ? null : valor);
    this.cargar();
  }

  limpiarDia(): void {
    if (this.dia() === null) {
      return;
    }
    this.dia.set(null);
    this.cargar();
  }

  inicial(nombre: string): string {
    return iniciales(nombre);
  }

  rolVisible(rol: string): string {
    return rolLegible(rol);
  }

  fecha(valor: string | null): string {
    return formatearFecha(valor);
  }

  detallePorteria(a: AprobacionPorteria): string {
    const partes = [`Puerta: ${a.puertaEntrada ?? '—'}`];
    if (a.motivoVisita) {
      partes.push(`Motivo: ${a.motivoVisita}`);
    }
    if (a.areaDestino) {
      partes.push(`Área: ${a.areaDestino}`);
    }
    return partes.join(' · ');
  }

  detalleParqueo(a: AprobacionParqueo): string {
    const partes = [`Puerta: ${a.puertaAcceso}`];
    const vehiculo = [a.marca, a.modelo].filter(Boolean).join(' ');
    if (vehiculo) {
      partes.push(vehiculo);
    }
    return partes.join(' · ');
  }
}