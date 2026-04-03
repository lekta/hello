# Lekta Hello

Unity инди игра. Жанр "ебенящий ужас", immersive horror, головоломка с социальной манипуляцией, мидкор. Игровой цикл: поиск скрытого в космосе - решение сюжета в воспоминаниях - бои с боссами (космос) - получение новых способностей

## Структура кода

```
Assets/Code/
  Boot/        — точка входа (RootEntry), композиция, Updater для не-MonoBehaviour
  Domain/      — модели, интерфейсы (Api/), расширения; asmdef "Base", без зависимостей
  Runtime/     — MonoBehaviour-логика; asmdef "Runtime", зависит от Base
    Cosmos/    — звёздное поле, камера, курсор, скрытые объекты
    Input/     — реализация IInput
    Save/      — сериализация сохранений
  Editor/      — кастомные инспекторы, DevGui
```

Граф зависимостей: `Boot → Runtime → Domain`. Domain ничего не знает о Runtime.

Важные классы:
Assets/Code/Runtime/Cosmos/CosmosController.cs - композит модуля космоса
Assets/Code/Editor/Tools/DevWindow.cs - отладочное окно


## Архитектура

- **GameContext** — статический сервис-локатор (Input, Save). Инициализируется в RootEntry.Setup().
- **CosmosController** — MonoBehaviour-хаб сцены, владеет менеджерами (bodies, hiddens, cursor, camera) и прокидывает Update.
- Менеджеры — plain C# классы с Init/Update, не наследуют MonoBehaviour.

## Данные

```
Assets/Data/
  Configs/  — тут все конфиги
  Scenes/   — тут сцены (космос и локации будут)
```

Сцена Cosmos - звёздное поле с курсором и скрытыми объектами

## Соглашения

- Неймспейсы: `LH.*` (LH.Domain, LH.Cosmos, LH.Boot и т.д.)
- Константы: `CAPS_UNDERSCORES`
- `Object` = `UnityEngine.Object`, `Random` = `UnityEngine.Random` (алиасы в using)
- Не ломать строки/сигнатуры до ~200 символов
- Комментарии — только когда логика неочевидна
