import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { formatearFecha, iniciales } from '../../../core/utils/fecha.util';
import { HistorialUsuario } from '../../../domain/models/historial';
import { UsuarioLista } from '../../../domain/models/usuario';
import { UdiEmptyComponent } from '../../../shared/components/udi-empty/udi-empty.component';
import { UdiLoadingComponent } from '../../../shared/components/udi-loading/udi-loading.component';
import { AdminService } from '../../services/admin.service';

@Component({
  selector: 'app-usuario-historial',
  standalone: true,
  imports: [UdiLoadingComponent, UdiEmptyComponent],
  templateUrl: './usuario-historial.component.html',
  styleUrl: './usuario-historial.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuarioHistorialComponent implements OnInit {
  readonly usuario = input.required<UsuarioLista>();
  readonly cerrado = output<void>();

  private readonly admin = inject(AdminService);
  private readonly destroyRef = inject(DestroyRef);

  readonly cargando = signal(true);
  readonly historial = signal<HistorialUsuario | null>(null);

  ngOnInit(): void {
    this.admin
      .obtenerHistorialUsuario(this.usuario().id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (h) => {
          this.historial.set(h);
          this.cargando.set(false);
        },
        error: () => this.cargando.set(false),
      });
  }

  inicial(nombre: string): string {
    return iniciales(nombre);
  }

  fecha(valor: string | null): string {
    return formatearFecha(valor);
  }

  esActivo(fechaSalida: string | null): boolean {
    return fechaSalida == null;
  }

  cerrar(): void {
    this.cerrado.emit();
  }
}
