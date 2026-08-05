import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { BehaviorSubject, debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { FILTROS_ESTADO, FUENTES, FuenteHistorial } from '../../../core/constants/app.constants';
import { formatearFecha, iniciales } from '../../../core/utils/fecha.util';
import { HistorialParqueo, HistorialPorteria } from '../../../domain/models/historial';
import { UdiEmptyComponent } from '../../../shared/components/udi-empty/udi-empty.component';
import { UdiLoadingComponent } from '../../../shared/components/udi-loading/udi-loading.component';
import { AdminService, RegistroHistorial } from '../../services/admin.service';

interface DetalleCampo {
  etiqueta: string;
  valor: string;
}

type TipoVista = (typeof FILTROS_ESTADO)[keyof typeof FILTROS_ESTADO];

@Component({
  selector: 'app-historial',
  standalone: true,
  imports: [UdiLoadingComponent, UdiEmptyComponent],
  templateUrl: './historial.component.html',
  styleUrl: './historial.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HistorialComponent implements OnInit, OnDestroy {
  private readonly admin = inject(AdminService);
  private readonly destruir = new Subject<void>();

  readonly FUENTES = FUENTES;
  readonly opcionesVista: { id: TipoVista; nombre: string }[] = [
    { id: FILTROS_ESTADO.todos, nombre: 'Todos' },
    { id: FILTROS_ESTADO.activos, nombre: 'Activos' },
    { id: FILTROS_ESTADO.completados, nombre: 'Completados' },
  ];

  readonly fuente = this.admin.fuente;
  readonly registros = toSignal(this.admin.historial$, {
    initialValue: [] as RegistroHistorial[],
  });
  readonly cargando = signal(true);

  private readonly busqueda$ = new BehaviorSubject<string>('');
  readonly termino = toSignal(this.busqueda$.pipe(debounceTime(250), distinctUntilChanged()), {
    initialValue: '',
  });

  readonly filtro = signal<TipoVista>(FILTROS_ESTADO.todos);
  readonly seleccionado = signal<RegistroHistorial | null>(null);

  readonly visibles = computed(() => this.aplicarFiltros(this.registros(), this.termino().trim(), this.filtro()));

  readonly camposDetalle = computed<DetalleCampo[]>(() => {
    const d = this.seleccionado();
    return d ? this.construirCampos(d) : [];
  });

  ngOnInit(): void {
    this.admin.historial$.pipe(takeUntil(this.destruir)).subscribe({
      next: () => this.cargando.set(false),
      error: () => this.cargando.set(false),
    });
  }

  cambiarFuente(fuente: FuenteHistorial): void {
    this.cargando.set(true);
    this.admin.cambiarFuente(fuente);
  }

  recargar(): void {
    this.cargando.set(true);
    this.admin.recargarHistorial();
  }

  buscar(valor: string): void {
    this.busqueda$.next(valor.replace(/\s+/g, ' ').trimStart());
  }

  cambiarFiltro(filtro: TipoVista): void {
    this.filtro.set(filtro);
  }

  verDetalle(registro: RegistroHistorial): void {
    this.seleccionado.set(registro);
  }

  cerrarDetalle(): void {
    this.seleccionado.set(null);
  }

  inicial(nombre: string): string {
    return iniciales(nombre);
  }

  esPorteria(registro: RegistroHistorial): registro is HistorialPorteria {
    return 'motivoVisita' in registro;
  }

  fechaEntrada(registro: RegistroHistorial): string {
    return this.esPorteria(registro) ? registro.fechaEntrada : registro.fechaIngreso;
  }

  fechaEntradaTexto(registro: RegistroHistorial): string {
    return formatearFecha(this.fechaEntrada(registro));
  }

  private aplicarFiltros(
    registros: RegistroHistorial[],
    termino: string,
    filtro: TipoVista
  ): RegistroHistorial[] {
    const q = termino.toLocaleLowerCase();
    return registros.filter((r) => {
      const coincide =
        q === '' ||
        r.nombreCompleto.toLocaleLowerCase().includes(q) ||
        r.documentoIdentidad.toLocaleLowerCase().includes(q);
      if (!coincide) {
        return false;
      }

      const activo = r.fechaSalida == null;
      if (filtro === FILTROS_ESTADO.activos) {
        return activo;
      }
      if (filtro === FILTROS_ESTADO.completados) {
        return !activo;
      }
      return true;
    });
  }

  private construirCampos(registro: RegistroHistorial): DetalleCampo[] {
    const tipo = this.esPorteria(registro) ? 'Portería' : 'Parqueo';

    if (this.esPorteria(registro)) {
      return [
        { etiqueta: 'Tipo de entrada', valor: tipo },
        { etiqueta: 'Carnet de identidad', valor: registro.documentoIdentidad },
        { etiqueta: 'Fecha de entrada', valor: formatearFecha(registro.fechaEntrada) },
        { etiqueta: 'Fecha de salida', valor: formatearFecha(registro.fechaSalida) },
        { etiqueta: 'Puerta de entrada', valor: registro.puertaEntrada ?? '—' },
        { etiqueta: 'Puerta de salida', valor: registro.puertaSalida ?? '—' },
        { etiqueta: 'Motivo de visita', valor: registro.motivoVisita ?? '—' },
        { etiqueta: 'Área de destino', valor: registro.areaDestino ?? '—' },
      ];
    }

    const parqueo = registro as HistorialParqueo;
    return [
      { etiqueta: 'Tipo de entrada', valor: tipo },
      { etiqueta: 'Carnet de identidad', valor: parqueo.documentoIdentidad },
      { etiqueta: 'Matrícula', valor: parqueo.matricula },
      { etiqueta: 'Marca', valor: parqueo.marca ?? '—' },
      { etiqueta: 'Modelo', valor: parqueo.modelo ?? '—' },
      { etiqueta: 'Color', valor: parqueo.color ?? '—' },
      { etiqueta: 'Fecha de ingreso', valor: formatearFecha(parqueo.fechaIngreso) },
      { etiqueta: 'Fecha de salida', valor: formatearFecha(parqueo.fechaSalida) },
      { etiqueta: 'Puerta de acceso', valor: parqueo.puertaAcceso },
      { etiqueta: 'Observaciones', valor: parqueo.observaciones ?? '—' },
    ];
  }

  ngOnDestroy(): void {
    this.destruir.next();
    this.destruir.complete();
  }
}