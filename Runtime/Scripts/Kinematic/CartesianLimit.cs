using UnityEngine;

namespace Preliy.Flange
{
    [System.Serializable]
    public record CartesianLimit
    {
        public float LinearSpeed
        {
            get => _linearSpeed;
            set => _linearSpeed = value;
        }
        public float LinearAcc => _linearAcc;
        public float RotationSpeed
        {
            get => _rotationSpeed;
            set => _rotationSpeed = value;
        }
        public float RotationAcc => _rotationAcc;

        [SerializeField]
        private float _linearSpeed = 5;
        [SerializeField]
        private float _linearAcc = 15;
        [SerializeField]
        private float _rotationSpeed = 180;
        [SerializeField]
        private float _rotationAcc = 900;

        public CartesianLimit(float linearSpeed, float linearAcc, float rotationSpeed, float rotationAcc)
        {
            _linearSpeed = linearSpeed;
            _linearAcc = linearAcc;
            _rotationSpeed = rotationSpeed;
            _rotationAcc = rotationAcc;
        }

        public static CartesianLimit Default => new CartesianLimit(3, 10, 180, 900);
        public static CartesianLimit Null => new CartesianLimit(0, 0, 0, 0);
    }
}
