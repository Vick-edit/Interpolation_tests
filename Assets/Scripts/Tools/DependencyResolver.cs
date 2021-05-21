using Utils.Dispatcher;
using Utils.EffectPool;
using Utils.Logging;
using Utils.PreprocessorDirectives;
using IEffectPool = Utils.EffectPool.IEffectPool;
using ILogger = Utils.Logging.ILogger;

namespace Tools
{
    /// <summary>
    ///     Класс, который знает, какому интерфейсу соответствует какой класс и как его строить,
    ///     по сути заменя конфигу/фабрике/DI есть недостатки, такие, как большач кодо генерация и быстрое разрастание класса,
    ///     но так же даёт гибкость, заменить реализацию интерфейса в проекте можно в одном месте, при этом
    ///     не используется рефлекшен, как стандартном DI, а значит можно без опаски использовать в апдейт и прочих часто вызываемых местах,
    ///     главное не создавать контейнеров данных, ну разве что только на слабых ссылках, как временный кэш объектов
    /// </summary>
    public static class DependencyResolver
    {
        /// <summary> Получить логгер </summary>
        public static ILogger GetLogger()
        {
            if(IsIt.Editor)
                return new UnityLogger();
            else
                return new MockLogger();
        }

        /// <summary> Получить инстанс диспатчера </summary>
        public static IDispatcher GetCachedDispatcher()
        {
            return DispatcherWrapper.Instance;
        }

        /// <summary> Получить инстанс пула эффектов </summary>
        public static IEffectPool GetCachedEffectPool()
        {
            return EffectPoolWrapper.Instance;
        }
    }
}