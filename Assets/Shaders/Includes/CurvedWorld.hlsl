#ifndef CURVEDWORLD_INCLUDED
#define CURVEDWORLD_INCLUDED

// Global variables set by C#
float _CurveStrength;
float _CurveXStrength;

// Helper to apply curve in Vertex Shader
// Call this AFTER transforming to World Space but BEFORE transforming to Clip Space
void ApplyCurvedWorld(inout float3 positionWS)
{
    // DISABLED per user request
    // float3 camPos = _WorldSpaceCameraPos;
    // float dist = positionWS.z - camPos.z;
    
    // float yOff = (dist * dist) * _CurveStrength;
    // float xOff = (dist * dist) * _CurveXStrength;
    
    // positionWS.y -= yOff;
    // positionWS.x += xOff;
}

#endif
