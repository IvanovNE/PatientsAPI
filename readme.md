# Patients API

REST API для управления данными пациентов по стандарту **FHIR HL7**.

## Для запуска в Docker:

```bash
# Клонировать репозиторий
git clone https://github.com/IvanovNE/PatientsAPI.git
cd PatientsAPI

# Запуск API и БД
docker-compose up -d --build
```

### API будет доступен по ссылке: http://localhost:5000/index.html