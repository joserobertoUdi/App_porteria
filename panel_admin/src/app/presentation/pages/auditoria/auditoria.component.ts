import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnDestroy,
  OnInit,
  signal,
} from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { iniciales } from '../../../core/utils/fecha.util';
import { esRolGuardia } from '../../../core/utils/rol.util';
import { UsuarioLista } from '../../../domain/models/usuario';
import { UdiEmptyComponent } from '../../../shared/components/udi-empty/udi-empty.component';
import { UdiLoadingComponent } from '../../../shared/components/udi-loading/udi-loading.component';
import { AdminService } from '../../services/admin.service';
import { AuditoriaGuardiaComponent } from './auditoria-guardia.component';

@Component({
  selector: 'app-auditoria',
  standalone: true,
  imports: [UdiLoadingComponent, UdiEmptyComponent, AuditoriaGuardiaComponent],
  templateUrl: './auditoria.component.html',
  styleUrl: './auditoria.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditoriaComponent implements OnInit, OnDestroy {
  private readonly admin = inject(AdminService);
  private readonly destruir = new Subject<void>();

  readonly usuarios = toSignal(this.admin.usuarios$, {
    initialValue: [] as UsuarioLista[],
  });
  readonly cargando = signal(true);

  private readonly busqueda$ = new BehaviorSubject<string>('');
  readonly termino = toSignal(this.busqueda$.pipe(debounceTime(250), distinctUntilChanged()), {
    initialValue: '',
  });

  readonly guardias = computed(() => {
    const q = this.termino().trim().toLocaleLowerCase();
    return this.usuarios()
      .filter((u) => esRolGuardia(u.rol))
      .filter(
        (u) =>
          q === '' ||
          u.nombreCompleto.toLocaleLowerCase().includes(q) ||
          u.documentoIdentidad.toLocaleLowerCase().includes(q)
      );
  });

  readonly seleccionado = signal<UsuarioLista | null>(null);

  ngOnInit(): void {
    this.admin.usuarios$.pipe(takeUntil(this.destruir)).subscribe({
      next: () => this.cargando.set(false),
      error: () => this.cargando.set(false),
    });
  }

  recargar(): void {
    this.cargando.set(true);
    this.admin.recargarUsuarios();
  }

  buscar(valor: string): void {
    this.busqueda$.next(valor.replace(/\s+/g, ' ').trimStart());
  }

  inicial(nombre: string): string {
    return iniciales(nombre);
  }

  abrir(guardia: UsuarioLista): void {
    this.seleccionado.set(guardia);
  }

  cerrar(): void {
    this.seleccionado.set(null);
  }

  ngOnDestroy(): void {
    this.destruir.next();
    this.destruir.complete();
  }
}