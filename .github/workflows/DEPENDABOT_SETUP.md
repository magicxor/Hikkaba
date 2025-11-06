# Настройка доступа Dependabot к секретам

## Проблема
PR от Dependabot не имеют доступа к секретам репозитория из соображений безопасности, что приводит к падению тестов.

## Решение
Создан отдельный workflow `on_dependabot_pr.yml`, который:
- Использует `pull_request_target` для доступа к секретам
- Проверяет, что PR создан именно Dependabot-ом (`github.actor == 'dependabot[bot]'`)
- Запускает те же тесты, что и обычные PR

## Настройка Dependabot Secrets

### Вариант 1: Использование обычных secrets (текущая реализация)
Workflow `on_dependabot_pr.yml` использует обычные secrets репозитория через `secrets: inherit`.

### Вариант 2: Настройка отдельных Dependabot secrets (опционально)
Если нужна дополнительная безопасность, можно настроить отдельные secrets для Dependabot:

1. Перейдите в **Settings** → **Secrets and variables** → **Dependabot**
2. Создайте следующие секреты:
   - `DOCKER_USERNAME`
   - `DOCKER_PASSWORD`
   - `GIST_AUTH_TOKEN`

3. Обновите `on_dependabot_pr.yml`, явно передав секреты:
```yaml
call_reusable_run_tests:
  name: run tests
  needs: check_dependabot
  if: needs.check_dependabot.outputs.is_dependabot == 'true'
  uses: ./.github/workflows/reusable_run_tests.yml
  secrets:
    DOCKER_USERNAME: ${{ secrets.DOCKER_USERNAME }}
    DOCKER_PASSWORD: ${{ secrets.DOCKER_PASSWORD }}
    GIST_AUTH_TOKEN: ${{ secrets.GIST_AUTH_TOKEN }}
```

## Безопасность
✅ **Безопасно**: Workflow проверяет `github.actor == 'dependabot[bot]'` перед запуском  
✅ **Безопасно**: Dependabot не может изменять workflow файлы  
✅ **Безопасно**: `pull_request_target` запускается в контексте базовой ветки, но с кодом из PR  

## Проверка
После мерджа этих изменений, следующий Dependabot PR должен:
1. Не запускать workflow `on_pull_request.yml` (будет пропущен)
2. Запустить workflow `on_dependabot_pr.yml` с доступом к секретам
3. Успешно пройти все тесты

## Альтернативные решения
Если по какой-то причине `pull_request_target` не подходит:

### Вариант A: Автоматическое одобрение Dependabot PR
Создать workflow, который автоматически одобряет и мерджит Dependabot PR после прохождения тестов.

### Вариант B: Сделать некоторые шаги опциональными
Изменить `reusable_run_tests.yml`, чтобы шаги с секретами не падали, если секреты недоступны:
```yaml
- name: Login to Docker Hub
  if: env.DOCKER_USERNAME != ''
  uses: docker/login-action@v3
  with:
    username: ${{ secrets.DOCKER_USERNAME }}
    password: ${{ secrets.DOCKER_PASSWORD }}
  env:
    DOCKER_USERNAME: ${{ secrets.DOCKER_USERNAME }}
```
