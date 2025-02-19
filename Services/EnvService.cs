using System;
using DotNetEnv;

namespace VisionHelper.Services
{
    public class EnvService
    {
        public static void LoadEnvVariables()
        {
            Env.Load();
        }

        public static string GetApiKey()
        {
            return Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Erro: OPENAI_API_KEY não foi encontrada no .env");
        }
    }
}