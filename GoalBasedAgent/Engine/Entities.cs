using Engine.MessagingSystem;

namespace Engine
{
    namespace Entity
    {
        abstract class BaseGameEntity : IEventHandler
        {
            //this is the next valid ID. Each time a BaseGameEntity is instantiated
            //this value is updated
            private static int nextValidId;
            //every entity has a unique identifying number
            protected int id;
            //ctor
            public BaseGameEntity()
            {
                // Lazy implementation as we know that Entities will not be saved/loaded.
                id = nextValidId++;
            }
            // Methods
            public abstract void Update();
            public abstract bool HandleMessage(Telegram message);
            public override string ToString()
            {
                return $"{GetType()} with id: {id}";
            }
        }
    }
}
