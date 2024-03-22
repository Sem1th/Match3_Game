using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace Match3 {
    public class Match3 : MonoBehaviour {

        public static Match3 instance;
        [SerializeField] int width = 8;
        [SerializeField] int height = 8;
        [SerializeField] float cellSize = 1f;
        [SerializeField] Vector3 originPosition = Vector3.zero;
        [SerializeField] bool debug = true;
        [SerializeField] Sprite cellGrid; 
        [SerializeField] Gem gemPrefab;
        [SerializeField] GemType[] gemTypes;

        [SerializeField] GemType[] gemSpecial;
        [SerializeField] Ease ease = Ease.InQuad;
        [SerializeField] GameObject explosion, hint, secret;
        [SerializeField] GameObject secretButton;
        
        
        [SerializeField] float hintDelay;
        bool isHintActive = false;
        float lastInputTime = 0f;
        InputReader inputReader;
        [SerializeField] AudioManager audioManager;
        
        bool isPlayerMove = false;
        GridSystem2D<GridObject<Gem>> grid;

        Vector2Int selectedGem = Vector2Int.one * -1;

        List<Gem> specialGemsToRemove = new List<Gem>();

        public void DeselectGem() => selectedGem = new Vector2Int(-1, -1);
        void SelectGem(Vector2Int gridPos) => selectedGem = gridPos;

        bool IsEmptyPosition(Vector2Int gridPosition) => grid.GetValue(gridPosition.x, gridPosition.y) == null;

        bool IsValidPosition(Vector2 gridPosition) {
            return gridPosition.x >= 0 && gridPosition.x < width && gridPosition.y >= 0 && gridPosition.y < height;
        }
        

        bool IsEmptyPosition(int x, int y) => grid.GetValue(x, y) == null;

        void Awake() {
            instance = this;
            inputReader = GetComponent<InputReader>();
        }
        
        
        void Start() {
            InitializeGrid();
            inputReader.Fire += OnSelectGem;
            var numberOfGemsToMatchList = new List<int> { 3, 4, 5};
            StartCoroutine(RunGameLoop(numberOfGemsToMatchList));
            Debug.Log("Start");
        }


        void FixedUpdate()
        {
            if (Time.time - lastInputTime > hintDelay && !isHintActive)
            {
                ShowHint();
            }
        }

        public void ResetInputTime()
        {
            lastInputTime = Time.time;
        }

        void OnDestroy() {
            inputReader.Fire -= OnSelectGem;
        } 

        void OnSelectGem() {
            var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));
            
            if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;
            
            Gem selectedGem = grid.GetValue(gridPos.x, gridPos.y)?.GetValue();
            
            if (selectedGem == null) return;
            
            if (selectedGem.isSpecial) {
                SpecialGemSelected();
            } else if (gridPos == this.selectedGem) {
                DeselectGem();
               audioManager.PlayDeselect();
            } else if (this.selectedGem == Vector2Int.one * -1) {
                SelectGem(gridPos);
                audioManager.PlayClick();
            } else {
               var numberOfGemsToMatchList = new List<int> { 3, 4, 5};
               isPlayerMove = true;
               ResetInputTime();
               
               StartCoroutine(SwapGems(this.selectedGem, gridPos));
            }
        } 

        bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        void ShowHint()
        {
                List<Vector2Int> possibleMoves = new List<Vector2Int>();

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        GridObject<Gem> gridObject = grid.GetValue(x, y);

                        if (gridObject != null)
                        {
                            List<Vector2Int> moves = CheckPossibleMoves(x, y);

                            if (moves.Count > 0)
                            {
                                possibleMoves.AddRange(moves);
                            }
                        }
                    }
                }

                if (possibleMoves.Count > 0)
                {
                    Vector2Int hintMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
                    SwapGems(selectedGem, hintMove);
                    // Подсветить целевую ячейку
                    HighlightGem(hintMove);
                    DeselectGem();
                    ResetInputTime();
                }
                else
                {
                    Debug.Log("No possible moves");
                }
            
        }

        List<Vector2Int> CheckPossibleMoves(int x, int y)
        {
            List<Vector2Int> moves = new List<Vector2Int>();

            if (IsValidPosition(x - 1, y) && IsValidPosition(x + 1, y))
            {
                if (CanSwapGems(new Vector2Int(x, y), new Vector2Int(x + 1, y)))
                {
                    moves.Add(new Vector2Int(x + 1, y));
                }

                if (CanSwapGems(new Vector2Int(x, y), new Vector2Int(x - 1, y)))
                {
                    moves.Add(new Vector2Int(x - 1, y));
                }
            }

            if (IsValidPosition(x, y - 1) && IsValidPosition(x, y + 1))
            {
                if (CanSwapGems(new Vector2Int(x, y), new Vector2Int(x, y + 1)))
                {
                    moves.Add(new Vector2Int(x, y + 1));
                }

                if (CanSwapGems(new Vector2Int(x, y), new Vector2Int(x, y - 1)))
                {
                    moves.Add(new Vector2Int(x, y - 1));
                }
            }

            return moves;
        }

        bool CanSwapGems(Vector2Int gem1, Vector2Int gem2)
        {
            GridObject<Gem> gridObject1 = grid.GetValue(gem1.x, gem1.y);
            GridObject<Gem> gridObject2 = grid.GetValue(gem2.x, gem2.y);

            if (gridObject1 != null && gridObject2 != null)
            {
                Gem gemComponent1 = gridObject1.GetValue() as Gem;
                Gem gemComponent2 = gridObject2.GetValue() as Gem;

                if (gemComponent1 != null && gemComponent2 != null)
                {
                    // Сохраняем временно первый камень
                    Gem tempGem = gemComponent1;

                    // Меняем местами камни в сетке
                    grid.SetValue(gem1.x, gem1.y, gridObject2);
                    grid.SetValue(gem2.x, gem2.y, gridObject1);

                    // Проверяем, есть ли совпадения после обмена
                    List<List<Vector2Int>> matches = FindMatches(3, isPlayerMove);

                    // Меняем обратно камни в сетке
                    grid.SetValue(gem1.x, gem1.y, gridObject1);
                    grid.SetValue(gem2.x, gem2.y, gridObject2);

                    gridObject1.SetValue(tempGem);

                    return matches.Count > 0;
                }
            }
            return false;
        }

        void HighlightGem(Vector2Int gemPosition)
        {
            Gem gem = grid.GetValue(gemPosition.x, gemPosition.y)?.GetValue()?.GetComponent<Gem>();

            // Если камень найден
            if (gem != null)
            {
                audioManager.PlayHint();
                HintVFX(gemPosition);

                List<Vector2Int> possibleMoves = CheckPossibleMoves(gemPosition.x, gemPosition.y);

                if (possibleMoves.Count > 0)
                {
                    Vector2Int hintMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
                    Gem replaceGem = grid.GetValue(hintMove.x, hintMove.y)?.GetValue()?.GetComponent<Gem>();

                    if (replaceGem != null)
                    {
                        StartCoroutine(PlayHintEffects(replaceGem));
                    }
                }
            }
        }

        public void SecretActivated() {
            if (secretButton != null)
            {
                secretButton.GetComponent<Button>().enabled = true;
                SecretButtonVFX();
            } else {
                return;
            }
        }

        public void ClickedSecretButton() {
            secretButton.GetComponent<Button>().enabled = false;
            StartCoroutine(AddMovies(4));
            SecretButtonVFX();
            Destroy(secretButton, 3);

        }

        IEnumerator AddMovies(int count)
        {
            if(count > 0) {
            yield return new WaitForSeconds(0.7f);
            ScoreManager.instance.CounterAddMoves();
            audioManager.PlayPop();
            StartCoroutine(AddMovies(count - 1));
            }
        }

        IEnumerator PlayHintEffects(Gem replaceGem)
        {
            yield return new WaitForSeconds(2.0f);
            audioManager.PlayHint();
            HintVFX(new Vector2Int((int)replaceGem.transform.position.x, (int)replaceGem.transform.position.y));
        }


        void SpecialGemSelected()
        {
            Vector2Int selectedGem = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

            if (!IsValidPosition(selectedGem) || IsEmptyPosition(selectedGem)) return;

            Gem selectedGemComponent = grid.GetValue(selectedGem.x, selectedGem.y)?.GetValue()?.GetComponent<Gem>();

            if (selectedGemComponent != null && selectedGemComponent.isSpecial)
            {
                foreach (GemType gemType in gemSpecial)
                {
                    if (gemType.type == Type.Special && gemType.specialGemType == selectedGemComponent.Type.specialGemType)
                    { 
                        switch (gemType.specialGemType)
                        {
                            case SpecialGemType.Bomb:
                                // Выполнить действия для специального камня типа "Bomb"
                                StartCoroutine (ExplodeAroundSpecialGem(selectedGemComponent));
                                break;
                            case SpecialGemType.Lines:
                                StartCoroutine (ExplodeLineSpecialGem(selectedGemComponent));
                                // Выполнить действия для специального камня типа "Lines"
                                break;
                            case SpecialGemType.Magic:
                                // Выполнить действия для специального камня типа "Magic"
                                StartCoroutine (FindAndExplodeRandomGem(selectedGemComponent));
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
        /* void SpecialGemActiveDel() {
            foreach (Gem specialGemToRemove in specialGemsToRemove)
            {
                switch (specialGemToRemove.Type.specialGemType)
                {
                    case SpecialGemType.Bomb:
                        StartCoroutine(ExplodeAroundSpecialGem(specialGemToRemove));
                        break;
                    case SpecialGemType.Lines:
                        StartCoroutine(ExplodeLineSpecialGem(specialGemToRemove));
                        break;
                    case SpecialGemType.Magic:
                        StartCoroutine(FindAndExplodeRandomGem(specialGemToRemove));
                        break;
                    default:
                        break;
                }
                ResetInputTime();
            }

            specialGemsToRemove.Clear();
        } */
        void SpecialGemActiveDel(List<Gem> specialGemsToRemove)
        {
            foreach (Gem specialGemToRemove in specialGemsToRemove)
            {
                switch (specialGemToRemove.Type.specialGemType)
                {
                    case SpecialGemType.Bomb:
                        StartCoroutine(ExplodeAroundSpecialGem(specialGemToRemove));
                        break;
                    case SpecialGemType.Lines:
                        StartCoroutine(ExplodeLineSpecialGem(specialGemToRemove));
                        break;
                    //case SpecialGemType.Magic:
                     //   StartCoroutine(FindAndExplodeRandomGem(specialGemToRemove));
                    //    break;
                    default:
                        break;
                }
                ResetInputTime();
            }

            specialGemsToRemove.Clear();
        }
    

      IEnumerator ExplodeAroundSpecialGem(Gem specialGem)
        {
            Vector2Int gridPos = grid.GetXY(specialGem.transform.position);
            List<List<Vector2Int>> matches = new List<List<Vector2Int>>();

            // Обойти все соседние ячейки вокруг специального камня
            for (int x = gridPos.x - 1; x <= gridPos.x + 1; x++)
            {
                for (int y = gridPos.y - 1; y <= gridPos.y + 1; y++)
                {
                    // Проверить, что ячейка существует и содержит камень
                    if (grid.IsValid(x, y) && !IsEmptyPosition(x, y))
                    {
                        GridObject<Gem> gridObject = grid.GetValue(x, y);
                        Gem gem = gridObject?.GetValue();
                        // Уничтожить камень и вызвать визуальный эффект взрыва
                        if (gem != null)
                        {
                            matches.Add(new List<Vector2Int> { new Vector2Int(x, y) });
                        } 
                    }
                }
                audioManager.PlayBonus();
                ResetInputTime();
            }

             

        yield return StartCoroutine(ExplodeGems(matches, true));
        yield return StartCoroutine(MakeFallGems());
        yield return StartCoroutine(FillEmptySpots());

        ScoreManager.instance.CheckerCounter();
        ScoreManager.instance.CheckingWinOrLose();
        } 


        IEnumerator ExplodeLineSpecialGem(Gem specialGem) {
        Vector2Int gridPos = grid.GetXY(specialGem.transform.position);
        List<List<Vector2Int>> matches = new List<List<Vector2Int>>();

        // Уничтожить все камни в горизонтальной линии
        for (int x = 0; x < grid.Width; x++) {
                Gem gem = (Gem)grid.GetValue(x, gridPos.y).GetValue();

                // Уничтожить камень и добавить его позицию к списку matches
                if (gem != null) {
                      
                    matches.Add(new List<Vector2Int> { new Vector2Int(x, gridPos.y) });
                }
        }

        // Уничтожить все камни в вертикальной линии
        for (int y = 0; y < grid.Height; y++) {
                Gem gem = (Gem)grid.GetValue(gridPos.x, y).GetValue();

                // Уничтожить камень и добавить его позицию к списку matches
                if (gem != null) {
   
                    matches.Add(new List<Vector2Int> { new Vector2Int(gridPos.x, y) });
                }
        }
        audioManager.PlayBonus();
        yield return StartCoroutine(ExplodeGems(matches, true));
        yield return StartCoroutine(MakeFallGems());
        yield return StartCoroutine(FillEmptySpots());

        ResetInputTime();

        ScoreManager.instance.CheckerCounter();
        ScoreManager.instance.CheckingWinOrLose(); 
    }

        IEnumerator FindAndExplodeRandomGem(Gem specialGem) {
        GemType gemTypeToMatch = specialGem.Type;
        Vector2Int gridPos = grid.GetXY(specialGem.transform.position);
        List<Vector2Int> matches = FindSingleGemsOfType(specialGem);

        if (matches.Count > 0) {
            Vector2Int randomMatch = matches[Random.Range(0, matches.Count)]; 
            audioManager.PlayBonus();
            yield return StartCoroutine(ExplodeGems(new List<List<Vector2Int>> { matches }));
            yield return StartCoroutine(MakeFallGems());
            yield return StartCoroutine(FillEmptySpots());

            ScoreManager.instance.CheckerCounter();
            ScoreManager.instance.CheckingWinOrLose();
            ResetInputTime();
        }
    }

    List<Vector2Int> FindSingleGemsOfType(Gem specialGem) {
        GemType randomGemType = gemTypes[Random.Range(0, gemTypes.Length)];
        List<Vector2Int> matches = new List<Vector2Int>();

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var gem = grid.GetValue(x, y)?.GetValue();

                if (gem != null && gem.Type == randomGemType) {
                    matches.Add(new Vector2Int(x, y));
                }
            }
        }
        if (matches.Count > 0)
    {
        // Добавляем specialGem в список matches
        Vector2Int gridPos = grid.GetXY(specialGem.transform.position);
        matches.AddRange(new List<Vector2Int> { new Vector2Int(gridPos.x, gridPos.y) });
    }
    else
    {
        audioManager.PlayNoMatch();
    }

    audioManager.PlayMatch();
    return matches;
    }
        IEnumerator RunGameLoop(List<int> numberOfGemsToMatchList) 
        {
            //yield return StartCoroutine(SwapGems(gridPosA, gridPosB));
            Debug.Log("Work");
            while (true)
            {
                bool hasMatches = false; // Флаг для проверки наличия совпадений

                foreach (int numberOfGemsToMatch in numberOfGemsToMatchList)
                {
                    List<List<Vector2Int>> matches;
                    
                    do
                    {
                        matches = FindMatches(numberOfGemsToMatch, isPlayerMove);

                        yield return StartCoroutine(ExplodeGems(matches));

                        if (matches.Count >= 3)
                        {
                            hasMatches = true; // Установка флага в true, если есть совпадения

                            Vector2Int centralGemPosition = matches[0][0];
                            int x = centralGemPosition.x;
                            int y = centralGemPosition.y;
                            SpawnSpecialGem(x, y);
                        }

                        yield return StartCoroutine(MakeFallGems());
                        yield return StartCoroutine(FillEmptySpots());
                        DeselectGem();
                        
                        
                        matches = FindMatches(numberOfGemsToMatch, isPlayerMove);
                        isPlayerMove = false;
                        ScoreManager.instance.CheckingWinOrLose();
                    }
                    while (matches.Count >= 3);

                }
                if (!hasMatches)
                {
                    break;
                }
            }
        }

        IEnumerator FillEmptySpots()
        {
            bool hasEmptySpots = false;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (grid.GetValue(x, y) == null)
                    {
                        hasEmptySpots = true;
                        CreateGem(x, y);
                        audioManager.PlayPop();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            if (hasEmptySpots)
            {
                var numberOfGemsToMatchList = new List<int> { 3, 4, 5 };
                yield return StartCoroutine(RunGameLoop(numberOfGemsToMatchList));
            }
        }

        IEnumerator MakeFallGems() {
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    if (grid.GetValue(x, y) == null) {
                        for (var i = y + 1; i < height; i++) {
                            if (grid.GetValue(x, i) != null) {
                                var gem = grid.GetValue(x, i).GetValue();
                                grid.SetValue(x, y, grid.GetValue(x, i));
                                grid.SetValue(x, i, null);
                                gem.transform
                                    .DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f)
                                    .SetEase(ease);
                                audioManager.PlayWoosh();
                                yield return new WaitForSeconds(0.1f);
                                break;
                            }
                        }
                    }
                }
            }
        }
       IEnumerator ExplodeGems(List<List<Vector2Int>> matches, bool addExtraPoints = false)
        {
            Debug.Log("Boom");
            audioManager.PlayPop();
            // Определение количества очков для добавления
            int pointsToAdd = addExtraPoints ? 3 : 1; 
            
            List<Gem> specialGemsToRemove = new List<Gem>(); // Создаем список для специальных камней
            
            foreach (var matchGroup in matches)
            {
                foreach (var match in matchGroup)
                {
                    var gem = grid.GetValue(match.x, match.y)?.GetValue();
                    if (gem != null)
                    {
                        ExplodeVFX(match);
                        gem.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);
                        yield return new WaitForSeconds(0.1f);

                        if (gem.isSpecial && !specialGemsToRemove.Contains(gem))
                        {
                            specialGemsToRemove.Add(gem);
                        }
                        gem.DestroyGem();
                        Destroy(gem.gameObject, 0.1f);
                        grid.SetValue(match.x, match.y, null);
                        
                        ScoreManager.instance.AddPoint();
                        if (addExtraPoints) 
                        {
                            ScoreManager.instance.AddPoint();
                        }
                    }
                }
            }

            if (specialGemsToRemove.Count >= 0)
            {
                SpecialGemActiveDel(specialGemsToRemove); // Передаем список специальных камней в метод SpecialGemActiveDel
            }
        }
        
        void HintVFX(Vector2Int gemPosition) {
        
            var fx = Instantiate(hint, transform);
            fx.transform.position = grid.GetWorldPositionCenter(gemPosition.x, gemPosition.y);
            Destroy(fx, 2f);
        }

        void ExplodeVFX(Vector2Int match) {
        
            var fx = Instantiate(explosion, transform);
            fx.transform.position = grid.GetWorldPositionCenter(match.x, match.y);
            Destroy(fx, 5f);
        }

        void SecretButtonVFX() {
        
            var fx = Instantiate(secret, transform);
            fx.transform.position = secretButton.GetComponent<RectTransform>().position;
            Destroy(fx, 3f);
        }
        
        List<List<Vector2Int>> FindMatches(int numberOfGemsToMatch, bool isPlayerMove)
        {
            var numberOfGemsToMatchList = new List<int> { 3, 4, 5};
            List<List<Vector2Int>> matches = new List<List<Vector2Int>>();
            

            // Horizontal
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - (numberOfGemsToMatch - 1); x++)
                {
                    List<Vector2Int> match = new List<Vector2Int>();
                    var firstGem = grid.GetValue(x, y)?.GetValue();

                    if (firstGem != null)
                    {
                        match.Add(new Vector2Int(x, y));

                        for (int i = 1; i < numberOfGemsToMatch; i++)
                        {
                            var nextGem = grid.GetValue(x + i, y)?.GetValue();

                            if (nextGem == null || firstGem.Type != nextGem.Type)
                            {
                                match.Clear();
                                break;
                            }
                            match.Add(new Vector2Int(x + i, y));
                        }

                        if (match.Count == numberOfGemsToMatch)
                        {
                            matches.Add(match);
                        }
                    }
                }
            }

            // Vertical
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - (numberOfGemsToMatch - 1); y++)
                {
                    List<Vector2Int> match = new List<Vector2Int>();
                    var firstGem = grid.GetValue(x, y)?.GetValue();

                    if (firstGem != null)
                    {
                        match.Add(new Vector2Int(x, y));

                        for (int i = 1; i < numberOfGemsToMatch; i++)
                        {
                            var nextGem = grid.GetValue(x, y + i)?.GetValue();

                            if (nextGem == null || firstGem.Type != nextGem.Type)
                            {
                                match.Clear();
                                break;
                            }

                            match.Add(new Vector2Int(x, y + i));
                        }

                        if (match.Count == numberOfGemsToMatch)
                        {
                            matches.Add(match);
                        }
                    }
                }
            }

            if (isPlayerMove && matches.Count == 0)
            {
                audioManager.PlayNoMatch();
            }
            else
            {
                if(isPlayerMove)
                audioManager.PlayMatch();
            }
            //Debug.Log("Number of Gems to Match: " + numberOfGemsToMatch.ToString());
            return matches;
        } 
        

       IEnumerator SwapGems(Vector2Int gridPosA, Vector2Int gridPosB) {

            if (Vector2Int.Distance(gridPosA, gridPosB) > 1.5) 
                yield break;

            var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
            var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);
            
            //README DOTween asset
            gridObjectA.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f)
                .SetEase(ease);
            gridObjectB.GetValue().transform
                .DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f)
                .SetEase(ease);
            
            grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
            grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);
            
            yield return new WaitForSeconds(0.5f);

            ScoreManager.instance.CheckerCounter();
            ScoreManager.instance.CheckingWinOrLose(); 

            var numberOfGemsToMatchList = new List<int> { 3, 4, 5};
            yield return StartCoroutine(RunGameLoop(numberOfGemsToMatchList));

        } 

        void InitializeGrid() {
            DeselectGem();
            grid = GridSystem2D<GridObject<Gem>>.VerticalGrid(width, height, cellSize, originPosition, cellGrid, debug);

            //grid.DrowDebugLines();
            
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    CreateGem(x, y);
                }
            }
        }

        void CreateGem(int x, int y, GemType gemType = null) {
        Gem gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
        if (gemType == null) {
            gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
        } else {
            gem.SetType(gemType);
            gem.isSpecial = true;
            
        }
        var gridObject = new GridObject<Gem>(grid, x, y);
        gridObject.SetValue(gem);
        grid.SetValue(x, y, gridObject);
    }

        void SpawnSpecialGem(int x, int y) {
            var specialGemType = gemSpecial[Random.Range(0, gemSpecial.Length)];
            CreateGem(x, y, specialGemType);
        }
    }
}
