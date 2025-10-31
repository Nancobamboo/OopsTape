half4 ColorCorrection(half4 color, half4 brightness, half4 contrast) {
    color *= brightness;color = max(0, color);
    return max(0, lerp(color, color * color, contrast - 1));
}
half3 ColorCorrection(half3 color, half brightness, half contrast) {
    color *= brightness;color = max(0, color);
    return max(0, lerp(color, color * color, contrast - 1));
}
half ColorCorrection(half alpha, half brightness, half contrast) {
    alpha *= brightness;alpha = max(0,alpha);
    return max(0, lerp(alpha, alpha * alpha, contrast - 1));
}

half4 ColorCorrection(half4 color, half4 contrast) {
    color = max(0, color);
    return max(0, lerp(color, color * color, contrast - 1));
}
half3 ColorCorrection(half3 color, half contrast) {
    color = max(0, color);
    return max(0, lerp(color, color * color, contrast - 1));
}
half ColorCorrection(half alpha, half contrast) {
    alpha = max(0,alpha);
    return max(0, lerp(alpha, alpha * alpha, contrast - 1));
}

half fastSin(half x) {
    x = frac(x) * 2 - 1;
    return (x * abs(x) - x) * 4;
}

half fastCos(half x) {
    x = frac(x + 0.25f) * 2 - 1;
    return (x * abs(x) - x) * 4;
}

half fastSmoothStep(half min, half max, half x)
{
    return saturate((x-min)/(max-min));
}
