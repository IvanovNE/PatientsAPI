using Microsoft.Extensions.DependencyInjection;
using PatientsAPI.Application.Common.Interfaces;
using PatientsAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PatientsAPI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPatientService, PatientService>();

            return services;
        }
    }
}
