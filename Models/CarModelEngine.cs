namespace nstuning_api.Models
{
    /// <summary>Factory-fitment link: which engines were normally fitted to a model. Used for suggestions only; never restricts the pick.</summary>
    public class CarModelEngine
    {
        public int ModelId { get; set; }
        public CarModel? Model { get; set; }

        public int EngineId { get; set; }
        public CarEngine? Engine { get; set; }
    }
}
