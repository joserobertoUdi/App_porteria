import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { BehaviorSubject, debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { iniciales } from '../../../core/utils/fecha.util';
import { UsuarioLista } from '../../../domain/models/usuario';
import { UdiEmptyComponent } from '../../../shared/components/udi-empty/udi-empty.component';
import { UdiLoadingComponent } from '../../../shared/components/udi-loading/udi-loading.component';
import { AdminService } from '../../services/admin.service';
import { UsuarioFormComponent, ModoUsuario } from './usuario-form.component';
import { UsuarioPasswordComponent } from './usuario-password.component';

type TipoDialogo = 'crear' | 'editar' | 'password';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [UdiLoadingComponent, UdiEmptyComponent, UsuarioFormComponent, UsuarioPasswordComponent],
  templateUrl: './usuarios.component.html',
  styleUrl: './usuarios.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuariosComponent implements OnInit, OnDestroy {
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

  readonly visibles = computed(() => {
    const q = this.termino().trim().toLocaleLowerCase();
    if (q === '') {
      return this.usuarios();
    }
    return this.usuarios().filter(
      (u) =>
        u.nombreCompleto.toLocaleLowerCase().includes(q) ||
        u.documentoIdentidad.toLocaleLowerCase().includes(q)
    );
  });

  readonly dialogo = signal<TipoDialogo | null>(null);
  readonly seleccionado = signal<UsuarioLista | null>(null);
  readonly modoFormulario = computed<ModoUsuario>(() =>
    this.dialogo() === 'editar' ? 'editar' : 'crear'
  );

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

  abrirCrear(): void {
    this.seleccionado.set(null);
    this.dialogo.set('crear');
  }

  abrirEditar(usuario: UsuarioLista): void {
    this.seleccionado.set(usuario);
    this.dialogo.set('editar');
  }

  abrirPassword(usuario: UsuarioLista): void {
    this.seleccionado.set(usuario);
    this.dialogo.set('password');
  }

  cerrarDialogo(): void {
    this.dialogo.set(null);
    this.seleccionado.set(null);
  }

  ngOnDestroy(): void {
    this.destruir.next();
    this.destruir.complete();
  }
}