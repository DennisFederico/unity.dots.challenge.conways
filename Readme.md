# Unity DOTS Community Challenge #1 Submission

This is my submission for the Unity DOTS Community Challenge #1 Hosted by [Turbo Makes Games](https://johnnyturbo.itch.io/)
Information about the challenge can be found [here](https://itch.io/jam/dots-challenge-1)

The challenge was to create a Conway's Game of Life simulation using Unity's Data Oriented Technology Stack (DOTS).

## Strategy

I did not have a lot of time to work on this project, so I decided to keep things simple. I created a System for spawning the cells
and another that runs the simulation. I used a DynamicBuffer to store the state of the cells and a Quad prefab to render them.

I knew I needed an array for the Grid of cells, to store a bool of whether the cell is alive or dead. 
I remembered from the recent Unity Dots Bootcamp how they used a DynamicBuffer 
to store the heatmap and calculated the new state based on a copy as a NativeArray, `toNativeArray` while changing the underlying 
of the buffer using `asNativearray` in a job, so I decided to use the same strategy to run the simulation to calculate the new state of
the cells.

For rendering I decided to use a `Quad` prefab with `URPMaterialPropertyBaseColor` and set it to black or green depending if the cell was
dead or alive.

Since the DynamicBuffer update approach was compatible with `IEntityJob` using the `[EntityIndexInQuery]` execute attribute as index for 
the array, I ended up updating DynamicBuffer and the Cell entities in the same job.

The thing that cost me the most was the actual spawn of the cells. First I used a loop doing `EntityManager.Instantiate` but that didn't perform
well at all... So I decided to use a `IJobParallelFor` in the SpawnSystem to populate the grid with random bool values (dead/alive) and then 
instantiate all the Cell entities with the EntityManager in one call `state.EntityManager.Instantiate(prefabs.CellColorPrefab, cellsBuffer.Length, Allocator.Temp)`
a use an `IJobEntity` with the DynamicBuffer of cellStates to set the Position and the Color of the cells.

The whole thing is triggered from the MonoBehaviour creating/destroying singletonEntities and using the `state.RequireForUpdate` to drive
whether or not a system should run.

I also had the idea to **Flip** the Quads 180º on X-Axis instead of changing the color of the material, but the performance was a little worse than
changing the colors and I dropped the idea.

The final submission contains two implementation of the simulation, a `MainThread` and `IEntityJob` approach that you can select before starting it. 

## Performance

I was able to simulate 1024x1024 (1.048.576) cells at 60fps on my Macbook 16-Core Intel Core i9.
![Mac-1024x1024.png](webimg%2FMac-1024x1024.png)

And 2048x2048 (4.194.304) cells at 20fps on my Macbook 16-Core Intel Core i9.
![Mac-2048x2048.png](webimg%2FMac-2048x2048.png)