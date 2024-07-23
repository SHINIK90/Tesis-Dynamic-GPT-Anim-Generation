using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NormalDistributionCalculator : MonoBehaviour
{
    public double mean = 0.0;
    public double stdDev = 1.0;
    public double value = 1.0;
    private float probability;
    private double pdfValue;

    void Start()
    {
        // pdfValue = NormalPDF(value, mean, stdDev);
        probability = NormalDistError(value, mean, stdDev);
        // Debug.Log($"The PDF value at {value} is {pdfValue:F5}");
        Debug.Log($"The probability of a value being above {value} is {probability:P2}");
        Debug.Log("Raw value is: " + probability);
    }

    // Calculate the PDF of the normal distribution
    public double NormalPDF(double x, double mean, double stdDev){
        double exponent = -Math.Pow(x - mean, 2) / (2 * Math.Pow(stdDev, 2));
        return (1 / (stdDev * Math.Sqrt(2 * Math.PI))) * Math.Exp(exponent);
    }
    public float NormalDistError(double x, double mean, double stdDev){
        return (float) (NormalPDF(x, mean, stdDev) / NormalPDF(0, mean, stdDev));
    }
    public float NormalDistErrorPenalty(double x, double stdDev){
        return (float) (NormalPDF(x, 0f, stdDev) / NormalPDF(0f, 0f, stdDev));
    }
}

