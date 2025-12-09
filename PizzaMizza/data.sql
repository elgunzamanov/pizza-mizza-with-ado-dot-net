USE PizzaMizza;

INSERT INTO Ingredients (Name)
VALUES ('Cheese'),
		 ('Tomato'),
		 ('Onion'),
		 ('Olives'),
		 ('Pepperoni');

INSERT INTO Sizes (Name)
VALUES ('Small'),
		 ('Medium'),
		 ('Large');

INSERT INTO Pizzas (Name, Type)
VALUES ('Margarita', 'Classic'),
		 ('Pepperoni Feast', 'Special');

INSERT INTO Pizzas_Ingredients (PizzaId, IngredientId)
VALUES (1, 1),
		 (1, 2),
		 (2, 1),
		 (2, 2),
		 (2, 5);

INSERT INTO Pizzas_Prices (PizzaId, SizeId, Price)
VALUES (1, 1, 5.99),
		 (1, 2, 7.99),
		 (1, 3, 9.99),
		 (2, 1, 6.99),
		 (2, 2, 8.99),
		 (2, 3, 10.99);

