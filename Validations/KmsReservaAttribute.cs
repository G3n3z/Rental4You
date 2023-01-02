using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;
using Rental4You.Models;

namespace Rental4You.Validations
{
    public class KmsReservaAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            var _context = validationContext.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

            var registo = (Registo)validationContext.ObjectInstance;
            if(_context == null || _context.Registos == null)
            {
                new ValidationResult("Nao foi possivel efetuar a operação");
            }
            if(registo.Tipo == RegistoType.LEVANTAMENTO)
            {
                return ValidationResult.Success;
            }
            var reserva = _context.Reservas.Include(r => r.Levantamento).FirstOrDefault(r => r.ReservaId == registo.ReservaId);

            if (reserva == null)
            {
                return ValidationResult.Success;
            }
            if(reserva.Levantamento != null && reserva.Levantamento.Kms > registo.Kms)
            {
                return new ValidationResult(GetErrorMessage());
            }
            
            _context.Entry(reserva).State = EntityState.Detached;
            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Kms inferiores aos kms do levantamento.";
        }
    }
}

