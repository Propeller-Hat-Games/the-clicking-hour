using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class GameManager : Node2D {
	private Godot.Collections.Array<Sprite2D> glassSprites;
	private int vie;
	private static List<int> liste_verre = new List<int> {0,1,2,3} ;
	private List<int> liste_choisie = new List<int>();

	private List<Entity> liste_entite = new List<Entity>();
	private bool porte_ouverte;

	private int quotat;
	private int quotat_actuel;

	private float difficulte;
	private int nb_max_entite;
	
	private Random random = new Random();
	private PackedScene entityScene;
	private bool isSpawning = false;

	public GlassType GetRandomGlassType()
	{
		var values = Enum.GetValues(typeof(GlassType));
		return (GlassType)values.GetValue(random.Next(values.Length));
	}

	public int Get_vie(){
		return vie;
	}

	public void Set_vie(int n){
		vie=vie+n;
	}

	public bool Game_over()
	{
		if (vie <= 0)
		{
			return true;
		}
		return false;
	}

	public void Choisie_liste(int n)
	{
		liste_choisie.Clear();
		Random rdm = new Random();
		int choix;
		if(n > 4)
		{
			n=4;
		}
		while (n != 0)
		{
			choix = rdm.Next(0,4);
			if (!liste_choisie.Contains(liste_verre[choix]))
			{
				liste_choisie.Add(liste_verre[choix]);
				n--;
			}
		}
	}

	public void Ferme_porte()
	{
		porte_ouverte=false;
		//joue_animation
	}

	public void Ouvre_porte()
	{
		porte_ouverte=true;
		//joue_animation
	}

	public List<int> Get_liste_choisie()
	{
		return liste_choisie;
	}

	public void Depasse_quotat()
	{
		if(quotat_actuel >= quotat)
		{
			Ferme_porte();
			liste_entite.Clear();
			difficulte=difficulte+0.5f;
			//attendre avant une nouvelle manche
			nb_max_entite=(int)Math.Round(nb_max_entite*difficulte);
			Creer_manche();
		}
	}

	public async void Creer_manche()
	{
		GD.Print($"Starting wave with {nb_max_entite} entities. Difficulty: {difficulte}");
		isSpawning = true;
		Ouvre_porte();
		
		for (int i = 0; i < nb_max_entite; i++)
		{
			SpawnEntity();
			await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		}
		
		isSpawning = false;
		CheckWaveFinished();
	}
	
	private void SpawnEntity()
	{
		if (entityScene == null) return;

		var entity = entityScene.Instantiate<Entity>();
		entity.Position = GetRandomSpawnPosition();
		entity.TreeExited += () => OnEntityTreeExited(entity);
		AddChild(entity);
		liste_entite.Add(entity);
	}

	private void OnEntityTreeExited(Entity entity)
	{
		if (liste_entite.Contains(entity))
		{
			liste_entite.Remove(entity);
		}
		
		if (!isSpawning)
		{
			CheckWaveFinished();
		}
	}

	private async void CheckWaveFinished()
	{
		if (liste_entite.Count == 0 && !isSpawning)
		{
			GD.Print("Wave finished! Preparing next wave...");
			Ferme_porte();
			
			// Wait 3 seconds before next wave
			await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
			
			difficulte += 0.2f;
			nb_max_entite = (int)Math.Max(5, 5 * difficulte);
			Creer_manche();
		}
	}

	private Vector2 GetRandomSpawnPosition()
	{
		var viewportRect = GetViewportRect();
		float margin = 100f;
		// Always spawn on Left side
		Vector2 pos = Vector2.Zero;
		pos.X = -margin;
		pos.Y = (float)GD.RandRange(0, viewportRect.Size.Y);
		return pos;
	}

	public override void _Ready() {
		var glassScene = GD.Load<PackedScene>("res://scenes/glass.tscn");
		var glassInstance = glassScene.Instantiate<Glass>();
		AddChild(glassInstance);

		glassSprites = glassInstance.GetSprites();
		
		GD.Print("Loaded sprites:");
		foreach (var sprite in glassSprites) {
			GD.Print($"- {sprite.Name}");
		}

		// Connect door signal
		var door = GetTree().GetFirstNodeInGroup("Door") as Door;
		if (door != null)
		{
			door.EntityEnteredDoor += OnEntityEnteredDoor;
		}
		
		entityScene = GD.Load<PackedScene>("res://scenes/environment/entity.tscn");
		nb_max_entite = 5;
		difficulte = 1.0f;
		
		// Start first wave
		Creer_manche();
	}

	private void OnEntityEnteredDoor(Entity entity)
	{
		GD.Print("Entity reached the door!");
		// You might want to decrease life here
		// Set_vie(-1);
	}
}
