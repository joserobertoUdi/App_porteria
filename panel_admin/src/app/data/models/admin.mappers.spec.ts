import {
  aprobacionesOperadorDto,
  historialParqueoDto,
  historialPorteriaDto,
} from './admin.mappers';

const registroPorteriaBase = {
  idRegistro: 2,
  nombreCompleto: 'Portero Parqueo',
  documentoIdentidad: '9999999',
  fechaEntrada: '2026-08-28T16:49:27.1366667',
  fechaSalida: '2026-08-28T17:09:02.6566667',
  puertaEntrada: 'puerta1',
  puertaSalida: 'puerta2',
  motivoVisita: 'auditoria e2e',
  areaDestino: 'Oficinas',
};

describe('admin.mappers', () => {
  describe('historialPorteriaDto', () => {
    it('mapea el operador que registró el ingreso', () => {
      const r = historialPorteriaDto({
        ...registroPorteriaBase,
        registradoPorNombre: 'Portero Portería',
        registradoPorDocumento: '7777777',
      });
      expect(r.registradoPorNombre).toBe('Portero Portería');
      expect(r.registradoPorDocumento).toBe('7777777');
    });

    it('normaliza operador ausente (registro pre-auditoría) a null', () => {
      const r = historialPorteriaDto(registroPorteriaBase as never);
      expect(r.registradoPorNombre).toBeNull();
      expect(r.registradoPorDocumento).toBeNull();
    });
  });

  describe('historialParqueoDto', () => {
    it('mapea el operador del parqueo', () => {
      const r = historialParqueoDto({
        idRegistro: 1,
        matricula: '1234ABC',
        marca: 'Toyota',
        modelo: null,
        color: null,
        nombreCompleto: 'Visitante Parqueo',
        documentoIdentidad: '5555555',
        fechaIngreso: '2026-08-28T17:20:00',
        fechaSalida: null,
        puertaAcceso: 'rampa',
        observaciones: null,
        registradoPorNombre: 'Portero Parqueo',
        registradoPorDocumento: '8888888',
      });
      expect(r.registradoPorNombre).toBe('Portero Parqueo');
      expect(r.matricula).toBe('1234ABC');
    });
  });

  describe('aprobacionesOperadorDto', () => {
    it('mapea el resumen y las secciones portería/parqueo', () => {
      const m = aprobacionesOperadorDto({
        usuarioId: 3,
        nombreCompleto: 'Portero Portería',
        documentoIdentidad: '7777777',
        rol: 'PorteroPorteria',
        desde: null,
        hasta: null,
        totalCompletadas: 1,
        totalPendientes: 1,
        porteria: [
          {
            idRegistro: 2,
            fechaEntrada: '2026-08-28T16:49:27.1366667',
            fechaSalida: '2026-08-28T17:09:02.6566667',
            puertaEntrada: 'puerta1',
            puertaSalida: 'puerta2',
            motivoVisita: 'auditoria e2e',
            areaDestino: '',
            visitanteNombre: 'Portero Parqueo',
            visitanteDocumento: '9999999',
            estaCompletada: true,
          },
          {
            idRegistro: 3,
            fechaEntrada: '2026-08-28T17:30:00',
            fechaSalida: null,
            puertaEntrada: 'puerta1',
            puertaSalida: null,
            motivoVisita: 'reunión',
            areaDestino: '',
            visitanteNombre: 'Visitante A',
            visitanteDocumento: '1111111',
            estaCompletada: false,
          },
        ],
        parqueo: [
          {
            idRegistro: 1,
            fechaIngreso: '2026-08-28T17:20:00',
            fechaSalida: null,
            puertaAcceso: 'rampa',
            matricula: '1234ABC',
            marca: 'Toyota',
            modelo: null,
            visitanteNombre: 'Visitante Parqueo',
            visitanteDocumento: '5555555',
            estaCompletada: false,
          },
        ],
      });

      expect(m.usuarioId).toBe(3);
      expect(m.nombreCompleto).toBe('Portero Portería');
      expect(m.rol).toBe('PorteroPorteria');
      expect(m.desde).toBeNull();
      expect(m.totalCompletadas).toBe(1);
      expect(m.totalPendientes).toBe(1);

      expect(m.porteria).toHaveSize(2);
      expect(m.porteria[0].estaCompletada).toBeTrue();
      expect(m.porteria[0].visitanteNombre).toBe('Portero Parqueo');
      expect(m.porteria[1].estaCompletada).toBeFalse();
      expect(m.porteria[1].visitanteDocumento).toBe('1111111');

      expect(m.parqueo).toHaveSize(1);
      expect(m.parqueo[0].matricula).toBe('1234ABC');
      expect(m.parqueo[0].estaCompletada).toBeFalse();
    });

    it('normaliza secciones ausentes a arreglo vacío', () => {
      const m = aprobacionesOperadorDto({
        usuarioId: 2,
        nombreCompleto: 'Portero Parqueo',
        documentoIdentidad: '8888888',
        rol: 'PorteroParqueo',
        desde: '2026-08-28T00:00:00',
        hasta: '2026-08-29T00:00:00',
        totalCompletadas: 0,
        totalPendientes: 0,
        porteria: undefined,
        parqueo: undefined,
      } as never);

      expect(m.porteria).toEqual([]);
      expect(m.parqueo).toEqual([]);
      expect(m.desde).toBe('2026-08-28T00:00:00');
    });
  });
});