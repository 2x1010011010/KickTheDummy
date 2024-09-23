// The MIT License
// Copyright © 2013 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// https://www.youtube.com/c/InigoQuilez
// https://iquilezles.org

// Simplex

float2 hash(float2 p)
{
	p = float2( dot(p,float2(127.1,311.7)), dot(p,float2(269.5,183.3)) );
	return -1.0 + 2.0*frac(sin(p)*43758.5453123);
}

float ff_snoise( in float2 p )
{
    const float K1 = 0.366025404; // (sqrt(3)-1)/2;
    const float K2 = 0.211324865; // (3-sqrt(3))/6;
	float2  i = floor(p + (p.x+p.y) * K1);
    float2  a = p - i + (i.x+i.y) * K2;
    float m = step(a.y,a.x); 
    float2  o = float2(m, 1.0 - m);
    float2  b = a - o + K2;
	float2  c = a - 1.0 + 2.0*K2;
    float3  h = max(0.5 - float3(dot(a,a), dot(b,b), dot(c,c) ), 0.0);
    h = h * h;
	float3  n = h * h * float3(dot(a, hash(i+0.0)), dot(b, hash(i+o)), dot(c, hash(i+1.0)));
    return dot(n, float3(70.0, 70.0, 70.0));
}

float ff_fnoise(in float2 p, in float2 layerShift)
{
    return ff_snoise(p + layerShift) * .5f + ff_snoise(p * 2 + layerShift * 4.0f) * .25f + ff_snoise(p * 4 + layerShift * 8.0f) * .125f;
}