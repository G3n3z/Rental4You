using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Rental4You.Data;
using Rental4You.Models;

namespace Rental4You.Validations
{
    public class MatriculaUniqueAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            var _context = validationContext.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

            var veiculo = (Veiculo)validationContext.ObjectInstance;
            if(_context == null || _context.Veiculos == null)
            {
                new ValidationResult("Nao foi possivel efetuar a operação");
            }
            var entity = _context.Veiculos.FirstOrDefault(v => v.Matricula.ToLower() == value.ToString().ToLower());

            if (entity != null && veiculo.Id != entity.Id)
            {
                return new ValidationResult(GetErrorMessage(value.ToString()));
            }
            if (veiculo.Id != 0 && entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return ValidationResult.Success;
        }

        public string GetErrorMessage(string matricula)
        {
            return $"A Matricula {matricula} já esta em uso.";
        }
    }
}

