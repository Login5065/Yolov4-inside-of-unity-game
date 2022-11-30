#ifndef EXTRA_H
#define EXTRA_H

#define MAX_DETECTION 512
#define ANCHOR_COUNT 3

struct DetectedObject
{
    float x, y, w, h;
    uint classIndex;
    float score;
};


#endif