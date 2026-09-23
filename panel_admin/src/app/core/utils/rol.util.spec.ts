import { esRolGuardia, rolVisible } from './rol.util';

describe('rol.util', () => {
  describe('esRolGuardia', () => {
    it('reconoce los roles de portería y parqueo', () => {
      expect(esRolGuardia('PorteroPorteria')).toBeTrue();
      expect(esRolGuardia('PorteroParqueo')).toBeTrue();
    });

    it('es insensible a mayúsculas y variaciones', () => {
      expect(esRolGuardia('porteroporteria')).toBeTrue();
      expect(esRolGuardia('PORTERO PARQUEO')).toBeTrue();
      expect(esRolGuardia(' Portero _ ')).toBeTrue();
    });

    it('rechaza roles que no son de guardia', () => {
      expect(esRolGuardia('Administrador')).toBeFalse();
      expect(esRolGuardia('Estudiante')).toBeFalse();
      expect(esRolGuardia('')).toBeFalse();
    });
  });

  describe('rolVisible', () => {
    it('traduce los nombres de rol del backend', () => {
      expect(rolVisible('PorteroPorteria')).toBe('Portero Portería');
      expect(rolVisible('PorteroParqueo')).toBe('Portero Parqueo');
      expect(rolVisible('Administrador')).toBe('Administrador');
    });

    it('deja pasar roles desconocidos sin cambios', () => {
      expect(rolVisible('OtroRol')).toBe('OtroRol');
    });
  });
});