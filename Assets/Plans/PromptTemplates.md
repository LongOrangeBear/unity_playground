# Prompt Templates for Unity AI

## 1. Basic Mechanic (PlayerController)

```
Ты senior Unity engineer. Сделай минимальную, поддерживаемую реализацию.

Контекст:
- Unity: 6.x, Input: New Input System, Pipeline: URP
- Сцена: <имя>, игрок: <GameObject name>, компоненты: Rigidbody + CapsuleCollider

Задача:
Напиши C# скрипт PlayerController:
- Движение по XZ (WASD), камера не нужна
- Прыжок Space: AddForce вверх
- Прыжок только если на земле: ground-check через Raycast (слой Ground)
- Все параметры (speed, jumpForce, groundCheckDistance) — [SerializeField]
- Логи только на ключевых событиях
- В конце: "как подключить в Unity" (куда повесить, слои/теги, Inspector)

Ограничения:
- KISS/YAGNI, один скрипт = одна ответственность
- Не использовать Find* в Update
- Код компилируемый, без псевдокода
```

---

## 2. Scene Actions (Concrete)

```
/run create a new 'Ground' and 'Background' GameObject, under a new parent 'Stage' GameObject.
The Ground should be square, 50m wide and centered at 0,0,0.
The background should be at the positive z end of the ground and should be 50m wide with a 16:9 ratio.
```

---

## 3. Mass Migration (Materials)

```
Найди все материалы в проекте, которые используют шейдер 'Legacy/Diffuse',
и переведи их на 'Universal Render Pipeline/Lit'.
Сделай отчёт: сколько найдено, сколько обновлено, где были ошибки.
После изменений проверь Console на warnings/errors.
```

---

## 4. Prefab Hygiene

```
Цель: привести префабы к чистой структуре.
1) Определи базовый prefab <BaseEnemy>
2) Сделай варианты (Prefab Variant) для <EnemyMelee>, <EnemyRanged>
3) Убери scene-level overrides, которые должны жить в prefab asset
4) Где нужны отличия — оставь overrides только на Variant
5) В конце: список изменённых prefab assets + что стало overrides и почему
```

---

## 5. UI Toolkit Screen

```
Собери UI Toolkit экран "Inventory":
- UXML: список предметов (ListView), панель деталей, кнопки Equip/Drop
- USS: никаких inline styles; используй BEM для классов
- Для элементов списка: продумай pooling/перепривязку данных
- Дай: структуру файлов (UXML/USS/C#), и короткий чеклист perf
```

---

## 8. Task Envelope (Universal Wrapper)

```
Ты — Unity Agent. Работай строго по AGENTS.md.

Цель: <что нужно получить>
Контекст проекта: Unity 6.x, URP, UI Toolkit/UGUI, Addressables да/нет

Ограничения:
- Не создавай нежелательные Prefab Overrides; если нужно — объясни и предложи Variant
- В UI Toolkit избегай inline styles; правь USS selectors, используй classList
- В Prefab Mode: Transform правь только в Isolation
- В Play Mode не мутируй Addressable assets; делай Instantiate

План работы:
1) Скан репозитория/сцены/префаба
2) Минимальный план изменений
3) Реализация
4) Проверка: компиляция, playmode sanity, отсутствие лишних overrides

Вывод: список изменённых файлов + что проверить в Editor
```

---

## 9. Multi-Scene Refactor

```
Цель: разделить текущую сцену на:
- Bootstrap (инициализация)
- Gameplay (уровень)
- UI (интерфейсы)
и настроить additive loading/unloading.

Требования:
- В Editor: сцены открываются одновременно (multi-scene)
- В рантайме: LoadSceneMode.Additive + UnloadSceneAsync
- Если есть light probes — вызывай LightProbes.Tetrahedralize()

Вывод:
- Какие сцены созданы/переименованы
- Какие объекты куда перенесены
- Какой код отвечает за загрузку/выгрузку
```

---

## 10. Addressables Setup

```
Нужно перевести <AssetSet> в Addressables.

Сделай:
- Стратегию группировки: concurrent usage (по уровням) как базовый вариант
- Учти: обновляемость, distribution (remote/local), performance, memory
- Guardrail: в Play Mode не менять исходный asset; только Instantiate копии

Вывод:
- Структура групп и rationale
- Риски по VCS конфликтам
- Чек-лист ручной проверки в Editor
```

---

## 6. UGUI Optimization

```
Оптимизируй текущий UGUI:
- Раздели UI на статический Canvas и динамический sub-canvas
- Убери Graphic Raycaster там, где нет интерактива
- Отключи Raycast Target у статических элементов
- Минимизируй layout groups, где можно
В конце: что изменено и почему это снижает Canvas rebuild cost.
```

---

## 7. Starter Super-Prompt (Universal)

```
Ты эксперт Unity + C#. Всегда:
1) KISS и YAGNI
2) SRP: отдельные скрипты по ответственности
3) Для сложного поведения — State pattern
4) Связь систем — C# events или UnityEvents
5) Конфиг-данные — ScriptableObjects
6) UI-текст — TextMeshPro
7) Логи на критических точках
8) В конце каждого скрипта — схема подключения в Unity
9) Модульность и сопровождаемость важнее "магии"

Сейчас: <конкретная задача>
```
