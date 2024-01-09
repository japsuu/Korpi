﻿using Korpi.Client.World.Regions.Chunks;
using Korpi.Client.World.Regions.Chunks.Blocks;
using OpenTK.Mathematics;

namespace ClientTests;

public class ChunkTests
{
    [Test]
    public void GetBlockState_ReturnsCorrectBlockState()
    {
        // Arrange
        Chunk chunk = new(new Vector3i(0, 0, 0));
        Vector3i position = new(1, 1, 1);
        BlockState blockState = new();
        chunk.SetBlockState(position, blockState, out _);

        // Act
        BlockState result = chunk.GetBlockState((position.X, position.Y, position.Z));

        // Assert
        Assert.That(result, Is.EqualTo(blockState));
    }
}