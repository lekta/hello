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
Вьюхи (..View) - всегда зависимы от логической части, но не наоборот. Должно быть можно сформировать тестовое окружение из логики, и оно будет работать не зная о вьюхах

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

Assets/Doc/_todo.txt - актуальные тудухи
Assets/Doc/_done.txt - сделанное, добавлять сверху краткое описание проделаной работы

## Соглашения

- Проделаную работу записывать в _done
- Неймспейсы: `LH.*` (LH.Domain, LH.Cosmos, LH.Boot и т.д.)
- Константы: `CAPS_UNDERSCORES`
- `Object` = `UnityEngine.Object`, `Random` = `UnityEngine.Random` (алиасы в using)
- Не ломать строки/сигнатуры до ~200 символов
- Комментарии — только когда логика неочевидна
- Если текущая структура кода жмёт - предложить архитектурно расшириться
