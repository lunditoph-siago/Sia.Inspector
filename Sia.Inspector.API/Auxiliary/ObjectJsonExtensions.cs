namespace Sia.Inspector.API;

using System.Text.Json;

public static class ObjectJsonExtensions
{
    public static IResult ToJsonResult<T>(this T obj)
        => Results.Content(JsonSerializer.Serialize(obj, SiaJsonOptions.Default), "application/json");

    public static IResult ToJsonResult<T>(this T obj, JsonSerializerOptions options)
        => Results.Content(JsonSerializer.Serialize(obj, options), "application/json");
}