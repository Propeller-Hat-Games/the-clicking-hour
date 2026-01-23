using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node2D {
	private Godot.Collections.Array<Sprite2D> glassSprites;
	private int vie;
	private static List<int> liste_verre = new List<int> {1,2,3,4} ;
	private List<int> liste_choisie;

	private List<Entity> liste_entite;
	private bool porte_ouverte;

	private int quotat;
	private int quotat_actuel;

	private float difficulte;
	private int nb_max_entite;

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
		foreach(Entity entite in liste_entite)
		{
			//entite.Die();
		}
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

	public void Creer_manche()
	{
		
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
	}
}
