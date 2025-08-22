{
  "variables":[
    {
      "name":"Health",
      "terms":[
        { "label":"Low",    "points":[0.0, 0.0, 0.3] },
        { "label":"Medium", "points":[0.2, 0.5, 0.8] },
        { "label":"High",   "points":[0.7, 1.0, 1.0] }
      ]
    }
  ],
  "rules":[
    {
      "conditions":[
        {"variable":"Health","term":"Low"}
      ],
      "actions":[
        {"variable":"SpawnMedkit","term":"High"}
      ]
    }
  ]
}
