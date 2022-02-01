using UnityEngine;

namespace Procedural
{
    public struct NoiseSample 
    {
        public float Height;
        public Vector3 Derivative;

        public NoiseSample(float height, Vector3 derivative)
        {
            Height = height;
            Derivative = derivative;
        }

		public Vector3 GetNormal(float noiseScale)
		{
			Vector2 scaledDerivative = Derivative / noiseScale;
			return (new Vector3(-scaledDerivative.x, 1, -scaledDerivative.y)).normalized;
		}

        public static NoiseSample operator + (NoiseSample a, NoiseSample b) 
        {
	    	a.Height += b.Height;
	    	a.Derivative += b.Derivative;
	    	return a;
	    }

        public static NoiseSample operator + (NoiseSample a, float b) 
        {
	    	a.Height += b;
	    	return a;
	    }

	    public static NoiseSample operator + (float a, NoiseSample b) 
        {
	    	b.Height += a;
	    	return b;
	    }

        public static NoiseSample operator - (NoiseSample a, float b) 
        {
		    a.Height -= b;
		    return a;
	    }
    
	    public static NoiseSample operator - (float a, NoiseSample b) 
        {
	    	b.Height = a - b.Height;
	    	b.Derivative = -b.Derivative;
	    	return b;
	    }
    
	    public static NoiseSample operator - (NoiseSample a, NoiseSample b) 
        {
	    	a.Height -= b.Height;
	    	a.Derivative -= b.Derivative;
	    	return a;
	    }

        public static NoiseSample operator * (NoiseSample a, float b) {
	    	a.Height *= b;
	    	a.Derivative *= b;
	    	return a;
	    }

	    public static NoiseSample operator * (float a, NoiseSample b) {
	    	b.Height *= a;
	    	b.Derivative *= a;
	    	return b;
	    }

        public static NoiseSample operator * (NoiseSample a, NoiseSample b) {
		    a.Derivative = a.Derivative * b.Height + b.Derivative * a.Height;
		    a.Height *= b.Height;
		    return a;
	    }
    }
}